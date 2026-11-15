namespace DEALER.DataAccess
{
    public class PreOrderManager : BaseDataManager, IPreOrder
    {
        public PreOrderManager(DEALERContext model) : base(model)
        {
        }

        // =====================================================================
        // অ্যাকাউন্টিং নিয়ম (সংক্ষেপে):
        //   পেমেন্ট দিলে    => Supplier এর due কমে (বেশি দিলে নেগেটিভ = অগ্রিম)
        //   ডেলিভারি পেলে   => Supplier এর due বাড়ে (পাওয়া পণ্যের দামে)
        //   ফলে ১০০০ পিসের টাকা দিয়ে ৯০০ পিস পেলে, ১০০ পিসের টাকা অগ্রিম হিসেবে থেকে যায়।
        //
        //   ডেলিভারি যদি অন্য কোনো Purchase এর সাথে আসে (PurchaseId দেওয়া থাকে),
        //   তাহলে stock আর due সেই Purchase নিজেই বাড়ায়, এখানে আবার বাড়ানো হয় না।
        //   শুধু PreOrderDetail এর DeliveredQuantity আপডেট হয়।
        // =====================================================================

        // ---------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------
        public bool CreatePreOrder(PreOrder preOrder)
        {
            if (preOrder == null || preOrder.PreOrderDetails == null || !preOrder.PreOrderDetails.Any())
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                // প্রথম পেমেন্ট আলাদা করে রাখো, পরে PreOrderPayment হিসেবে সেভ হবে
                var initialPayment = preOrder.TotalPaid;
                preOrder.TotalPaid = 0;
                preOrder.Status = PreOrderStatus.Pending;

                foreach (var detail in preOrder.PreOrderDetails)
                {
                    detail.DeliveredQuantity = 0;
                    detail.CancelledQuantity = 0;
                }

                _dbContext.PreOrders.Add(preOrder);
                _dbContext.SaveChanges(); // Id তৈরির জন্য

                if (string.IsNullOrWhiteSpace(preOrder.OrderNumber))
                {
                    preOrder.OrderNumber = $"PO-{preOrder.OrderDate:yyyy}-{preOrder.Id:D4}";
                    _dbContext.SaveChanges();
                }

                if (initialPayment > 0)
                {
                    AddPaymentCore(preOrder, new PreOrderPayment
                    {
                        PreOrderId = preOrder.Id,
                        Amount = initialPayment,
                        PaymentMethodId = preOrder.PaymentMethodId,
                        Date = preOrder.OrderDate,
                        Comments = "Initial payment"
                    });
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------
        public bool UpdatePreOrder(PreOrder preOrder)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var existing = _dbContext.PreOrders
                    .Include(o => o.PreOrderDetails)
                    .FirstOrDefault(o => o.Id == preOrder.Id && !o.IsDeleted);

                if (existing == null)
                    return false;

                if (existing.Status == PreOrderStatus.Cancelled)
                    throw new InvalidOperationException("Cancelled pre-order edit kora jabe na.");

                var hasActiveDelivery = _dbContext.PreOrderDeliveries
                    .Any(d => d.PreOrderId == existing.Id && !d.IsDeleted);

                // ডেলিভারি শুরু হয়ে গেলে পণ্য/দাম বদলানো যাবে না
                // (কারণ পুরনো ডেলিভারির due হিসাব আগের দামে হয়ে গেছে)
                if (hasActiveDelivery)
                {
                    if (Math.Abs(existing.TotalAmount - preOrder.TotalAmount) > 0.001)
                        throw new InvalidOperationException("Delivery hoye gele order-er product ba dam change kora jabe na.");

                    existing.OrderDate = preOrder.OrderDate;
                    existing.ExpectedDeliveryDate = preOrder.ExpectedDeliveryDate;
                    existing.Comments = preOrder.Comments;

                    _dbContext.SaveChanges();
                    transaction.Commit();
                    return true;
                }

                // Supplier বদলানো যাবে না যদি পেমেন্ট হয়ে যায় (Supplier ledger নষ্ট হবে)
                var hasPayment = _dbContext.PreOrderPayments.Any(p => p.PreOrderId == existing.Id && !p.IsDeleted);
                if (hasPayment && existing.SupplierId != preOrder.SupplierId)
                    throw new InvalidOperationException("Payment deya thakle Supplier change kora jabe na.");

                existing.SupplierId = preOrder.SupplierId;
                existing.OrderDate = preOrder.OrderDate;
                existing.ExpectedDeliveryDate = preOrder.ExpectedDeliveryDate;
                existing.Comments = preOrder.Comments;
                existing.PaymentMethodId = preOrder.PaymentMethodId;
                existing.SubTotal = preOrder.SubTotal;
                existing.Discount = preOrder.Discount;
                existing.TotalAmount = preOrder.TotalAmount;

                // --- Details sync ---
                var incomingIds = preOrder.PreOrderDetails.Where(d => d.Id > 0).Select(d => d.Id).ToList();

                // যেগুলো আর নেই, সেগুলো মুছো
                var removed = existing.PreOrderDetails.Where(d => !incomingIds.Contains(d.Id)).ToList();
                foreach (var r in removed)
                {
                    if (r.CancelledQuantity > 0 || _dbContext.PreOrderDeliveryDetails.Any(x => x.PreOrderDetailId == r.Id))
                        throw new InvalidOperationException("Delivery/Cancel hoye gese emon item remove kora jabe na.");

                    _dbContext.PreOrderDetails.Remove(r);
                }

                foreach (var incoming in preOrder.PreOrderDetails)
                {
                    var current = existing.PreOrderDetails.FirstOrDefault(d => d.Id == incoming.Id && incoming.Id > 0);

                    if (current == null)
                    {
                        existing.PreOrderDetails.Add(new PreOrderDetail
                        {
                            ProductId = incoming.ProductId,
                            OrderedQuantity = incoming.OrderedQuantity,
                            DeliveredQuantity = 0,
                            CancelledQuantity = 0,
                            CylinderUnitPrice = incoming.CylinderUnitPrice,
                            GasUnitPrice = incoming.GasUnitPrice,
                            Discount = incoming.Discount,
                            Price = incoming.Price
                        });
                    }
                    else
                    {
                        current.ProductId = incoming.ProductId;
                        current.OrderedQuantity = incoming.OrderedQuantity;
                        current.CylinderUnitPrice = incoming.CylinderUnitPrice;
                        current.GasUnitPrice = incoming.GasUnitPrice;
                        current.Discount = incoming.Discount;
                        current.Price = incoming.Price;
                    }
                }

                UpdatePreOrderStatus(existing);
                _dbContext.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------
        public bool DeletePreOrder(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var preOrder = _dbContext.PreOrders.FirstOrDefault(o => o.Id == id);
                if (preOrder == null)
                    return false;

                if (preOrder.IsDeleted)
                    return true; // আগে থেকেই deleted

                var hasActiveDelivery = _dbContext.PreOrderDeliveries.Any(d => d.PreOrderId == id && !d.IsDeleted);
                if (hasActiveDelivery)
                    throw new InvalidOperationException("Delivery thakle order delete kora jabe na. Age delivery delete koro.");

                // সব পেমেন্ট reverse করো
                var payments = _dbContext.PreOrderPayments
                    .Where(p => p.PreOrderId == id && !p.IsDeleted)
                    .ToList();

                foreach (var payment in payments)
                {
                    DeletePaymentCore(preOrder, payment);
                }

                preOrder.IsDeleted = true;
                _dbContext.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // বাকি পিস বাতিল (কোম্পানি আর দেবে না)
        // দ্রষ্টব্য: দেওয়া টাকা Supplier এর কাছে অগ্রিম হিসেবেই থাকবে
        // ---------------------------------------------------------------
        public bool CancelPendingQuantity(int preOrderDetailId, int quantity)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var detail = _dbContext.PreOrderDetails.FirstOrDefault(d => d.Id == preOrderDetailId);
                if (detail == null)
                    return false;

                if (quantity <= 0 || quantity > detail.PendingQuantity)
                    throw new InvalidOperationException($"Cancel quantity 1 theke {detail.PendingQuantity} er moddhe hote hobe.");

                detail.CancelledQuantity += quantity;

                var preOrder = _dbContext.PreOrders
                    .Include(o => o.PreOrderDetails)
                    .First(o => o.Id == detail.PreOrderId);

                UpdatePreOrderStatus(preOrder);
                _dbContext.SaveChanges();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // PAYMENT
        // ---------------------------------------------------------------
        public bool AddPayment(PreOrderPayment payment)
        {
            if (payment == null || payment.Amount <= 0)
                return false;

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var preOrder = _dbContext.PreOrders.FirstOrDefault(o => o.Id == payment.PreOrderId && !o.IsDeleted);
                if (preOrder == null)
                    return false;

                if (preOrder.Status == PreOrderStatus.Cancelled)
                    throw new InvalidOperationException("Cancelled pre-order e payment deya jabe na.");

                AddPaymentCore(preOrder, payment);

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public bool DeletePayment(int paymentId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var payment = _dbContext.PreOrderPayments.FirstOrDefault(p => p.Id == paymentId);
                if (payment == null)
                    return false;

                if (payment.IsDeleted)
                    return true;

                var preOrder = _dbContext.PreOrders.First(o => o.Id == payment.PreOrderId);

                DeletePaymentCore(preOrder, payment);

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // DELIVERY
        // ---------------------------------------------------------------
        public bool AddDelivery(PreOrderDelivery delivery)
        {
            if (delivery == null || delivery.PreOrderDeliveryDetails == null || !delivery.PreOrderDeliveryDetails.Any())
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var preOrder = _dbContext.PreOrders
                    .Include(o => o.PreOrderDetails)
                    .FirstOrDefault(o => o.Id == delivery.PreOrderId && !o.IsDeleted);

                if (preOrder == null)
                    return false;

                if (preOrder.Status == PreOrderStatus.Cancelled)
                    throw new InvalidOperationException("Cancelled pre-order e delivery neya jabe na.");

                // অন্য Purchase এর সাথে এলে, সেটা একই Supplier এর কিনা দেখো
                if (delivery.PurchaseId.HasValue)
                {
                    var purchaseOk = _dbContext.Purchases.Any(p =>
                        p.Id == delivery.PurchaseId.Value
                        && p.SupplierId == preOrder.SupplierId
                        && !p.IsDeleted);

                    if (!purchaseOk)
                        throw new InvalidOperationException("Purchase ti ei Supplier-er na ba paoa jai nai.");
                }

                // Header discount ke proportionally line value te distribute kora hoy,
                // jate pura delivery hole total value == TotalAmount hoy
                double factor = preOrder.SubTotal > 0 ? preOrder.TotalAmount / preOrder.SubTotal : 1;
                double deliveredValue = 0;

                foreach (var item in delivery.PreOrderDeliveryDetails)
                {
                    var detail = preOrder.PreOrderDetails.FirstOrDefault(d => d.Id == item.PreOrderDetailId);
                    if (detail == null)
                        throw new InvalidOperationException("Delivery item ei order-er na.");

                    if (item.Quantity <= 0)
                        throw new InvalidOperationException("Delivery quantity 0 er beshi hote hobe.");

                    if (item.Quantity > detail.PendingQuantity)
                        throw new InvalidOperationException($"Pending quantity ({detail.PendingQuantity}) er beshi delivery neya jabe na.");

                    item.ProductId = detail.ProductId;
                    detail.DeliveredQuantity += item.Quantity;

                    if (detail.OrderedQuantity > 0)
                        deliveredValue += detail.Price / detail.OrderedQuantity * item.Quantity * factor;
                }

                deliveredValue = Math.Round(deliveredValue, 2);

                _dbContext.PreOrderDeliveries.Add(delivery);
                UpdatePreOrderStatus(preOrder);
                _dbContext.SaveChanges();

                // সরাসরি ডেলিভারি হলে stock আর Supplier due এখানেই আপডেট হবে
                if (!delivery.PurchaseId.HasValue)
                {
                    foreach (var item in delivery.PreOrderDeliveryDetails)
                    {
                        var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == item.ProductId);
                        if (stock != null)
                        {
                            stock.LoadedQty += item.Quantity;
                        }
                    }
                    _dbContext.SaveChanges();

                    double dueBefore = GetLastSupplierDue(preOrder.SupplierId);

                    var ledger = new SupplierPaymentHistory()
                    {
                        SupplierId = preOrder.SupplierId,
                        PreOrderDeliveryId = delivery.Id,
                        PaymentDate = delivery.DeliveryDate,
                        Comments = delivery.Comments ?? $"Pre-order delivery ({preOrder.OrderNumber})",
                        Number = delivery.ChallanNumber,
                        TotalAmountThisPurchase = deliveredValue,
                        AmountPaid = 0,
                        TotalDueBeforePayment = dueBefore,
                        TotalDueAfterPayment = dueBefore + deliveredValue
                    };

                    _dbContext.SupplierPaymentHistories.Add(ledger);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public bool DeleteDelivery(int deliveryId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var delivery = _dbContext.PreOrderDeliveries
                    .Include(d => d.PreOrderDeliveryDetails)
                    .FirstOrDefault(d => d.Id == deliveryId);

                if (delivery == null)
                    return false;

                if (delivery.IsDeleted)
                    return true; // double-reverse এড়ানো

                var preOrder = _dbContext.PreOrders
                    .Include(o => o.PreOrderDetails)
                    .First(o => o.Id == delivery.PreOrderId);

                delivery.IsDeleted = true;

                foreach (var item in delivery.PreOrderDeliveryDetails)
                {
                    var detail = preOrder.PreOrderDetails.FirstOrDefault(d => d.Id == item.PreOrderDetailId);
                    if (detail != null)
                    {
                        detail.DeliveredQuantity = Math.Max(0, detail.DeliveredQuantity - item.Quantity);
                    }

                    if (!delivery.PurchaseId.HasValue)
                    {
                        var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == item.ProductId);
                        if (stock != null)
                        {
                            stock.LoadedQty -= item.Quantity;
                        }
                    }
                }

                UpdatePreOrderStatus(preOrder);
                _dbContext.SaveChanges();

                if (!delivery.PurchaseId.HasValue)
                {
                    var ledger = _dbContext.SupplierPaymentHistories
                        .FirstOrDefault(p => p.PreOrderDeliveryId == delivery.Id && !p.IsDeleted);

                    if (ledger != null)
                    {
                        ledger.IsDeleted = true;
                        RecalculateSupplierPaymentHistories(
                            ledger.SupplierId,
                            ledger.Id,
                            (double)(ledger.TotalAmountThisPurchase - ledger.AmountPaid));

                        _dbContext.SaveChanges();
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------
        public async Task<PreOrder> GetPreOrderById(int preOrderId)
        {
            try
            {
                return await _dbContext.PreOrders
                    .Include(o => o.Supplier)
                    .Include(o => o.PreOrderDetails)
                        .ThenInclude(d => d.Product)
                    .Include(o => o.PreOrderPayments.Where(p => !p.IsDeleted))
                    .Include(o => o.PreOrderDeliveries.Where(d => !d.IsDeleted))
                        .ThenInclude(d => d.PreOrderDeliveryDetails)
                    .FirstOrDefaultAsync(o => o.Id == preOrderId && !o.IsDeleted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<IEnumerable<PreOrder>> GetAllPreOrders()
        {
            try
            {
                return await _dbContext.PreOrders
                    .AsNoTracking()
                    .Include(o => o.Supplier)
                    .Include(o => o.PreOrderDetails)
                    .Where(o => !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .ThenByDescending(o => o.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<IEnumerable<PreOrder>> GetPendingPreOrders(int? supplierId = null)
        {
            try
            {
                var query = _dbContext.PreOrders
                    .AsNoTracking()
                    .Include(o => o.Supplier)
                    .Include(o => o.PreOrderDetails)
                        .ThenInclude(d => d.Product)
                    .Where(o => !o.IsDeleted
                        && (o.Status == PreOrderStatus.Pending || o.Status == PreOrderStatus.PartiallyDelivered));

                if (supplierId.HasValue)
                    query = query.Where(o => o.SupplierId == supplierId.Value);

                return await query
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // Helper methods
        // ---------------------------------------------------------------

        // Transaction শুরু না করেই পেমেন্ট সেভ (Create আর AddPayment দুটোই এটা ব্যবহার করে)
        private void AddPaymentCore(PreOrder preOrder, PreOrderPayment payment)
        {
            payment.PreOrderId = preOrder.Id;
            payment.IsDeleted = false;

            _dbContext.PreOrderPayments.Add(payment);
            preOrder.TotalPaid += payment.Amount;
            _dbContext.SaveChanges(); // payment.Id তৈরির জন্য

            // --- Supplier ledger: পেমেন্টে due কমে ---
            double dueBefore = GetLastSupplierDue(preOrder.SupplierId);

            var ledger = new SupplierPaymentHistory()
            {
                SupplierId = preOrder.SupplierId,
                PreOrderPaymentId = payment.Id,
                PaymentDate = payment.Date,
                PaymentMethodId = payment.PaymentMethodId,
                Comments = payment.Comments ?? $"Pre-order payment ({preOrder.OrderNumber})",
                TotalAmountThisPurchase = 0,
                AmountPaid = payment.Amount,
                TotalDueBeforePayment = dueBefore,
                TotalDueAfterPayment = dueBefore - payment.Amount
            };

            _dbContext.SupplierPaymentHistories.Add(ledger);
            _dbContext.SaveChanges();

            // --- Cash/Bank balance: টাকা বের হয়ে গেছে ---
            var existCurrentBalance = _dbContext.TransactionHistories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault()?.CurrentBalance ?? 0;

            var transactionHistory = new TransactionHistory()
            {
                BalanceIn = 0,
                BalanceOut = payment.Amount,
                CurrentBalance = existCurrentBalance - payment.Amount,
                Date = payment.Date,
                PreOrderPaymentId = payment.Id,
                Resone = $"Pre-order payment ({preOrder.OrderNumber})."
            };

            _dbContext.Add(transactionHistory);
            _dbContext.SaveChanges();
        }

        // Transaction শুরু না করেই পেমেন্ট reverse
        private void DeletePaymentCore(PreOrder preOrder, PreOrderPayment payment)
        {
            payment.IsDeleted = true;
            preOrder.TotalPaid -= payment.Amount;

            var ledger = _dbContext.SupplierPaymentHistories
                .FirstOrDefault(p => p.PreOrderPaymentId == payment.Id && !p.IsDeleted);

            if (ledger != null)
            {
                ledger.IsDeleted = true;
                RecalculateSupplierPaymentHistories(
                    ledger.SupplierId,
                    ledger.Id,
                    (double)(ledger.TotalAmountThisPurchase - ledger.AmountPaid));
            }

            var txn = _dbContext.TransactionHistories
                .FirstOrDefault(t => t.PreOrderPaymentId == payment.Id && !t.IsDeleted);

            if (txn != null)
            {
                if (txn.BalanceOut.HasValue && txn.BalanceOut.Value > 0)
                {
                    BalanceInTransactionHistories(txn.Id, txn.BalanceOut.Value);
                }

                txn.IsDeleted = true;
                _dbContext.Update(txn);
            }

            _dbContext.SaveChanges();
        }

        // অর্ডারের অবস্থা হিসাব করে বসাও (PreOrderDetails আগে Include করা থাকতে হবে)
        private void UpdatePreOrderStatus(PreOrder preOrder)
        {
            var details = preOrder.PreOrderDetails?.ToList();
            if (details == null || !details.Any())
                return;

            var totalDelivered = details.Sum(d => d.DeliveredQuantity);
            var totalPending = details.Sum(d => d.PendingQuantity);

            if (totalPending <= 0)
            {
                // সব পাওয়া গেছে, অথবা কিছুই পাওয়া যায়নি কিন্তু সব বাতিল
                preOrder.Status = totalDelivered > 0 ? PreOrderStatus.Delivered : PreOrderStatus.Cancelled;
            }
            else
            {
                preOrder.Status = totalDelivered > 0 ? PreOrderStatus.PartiallyDelivered : PreOrderStatus.Pending;
            }
        }

        private double GetLastSupplierDue(int supplierId)
        {
            return _dbContext.SupplierPaymentHistories
                .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .Select(p => (double?)p.TotalDueAfterPayment)
                .FirstOrDefault() ?? 0;
        }

        // PurchaseManager এর মতোই: পরের সব ledger row এর due সামঞ্জস্য করো
        private void RecalculateSupplierPaymentHistories(int supplierId, int id, double amount)
        {
            if (amount == 0)
                return;

            var payments = _dbContext.SupplierPaymentHistories
                .Where(p => p.SupplierId == supplierId && p.Id > id)
                .OrderBy(p => p.Id)
                .ToList();

            foreach (var payment in payments)
            {
                payment.TotalDueBeforePayment -= amount;
                payment.TotalDueAfterPayment -= amount;
            }

            _dbContext.SaveChanges();
        }
    }
}