namespace DEALER.DataAccess
{
    public class PreOrderManager : BaseDataManager, IPreOrder
    {
        public PreOrderManager(DEALERContext model) : base(model)
        {
        }

        // =====================================================================
        // অ্যাকাউন্টিং নিয়ম:
        //   পেমেন্ট দিলে    => Supplier এর due কমে
        //   ডেলিভারি পেলে   => Supplier এর due বাড়ে (পাওয়া পণ্যের দামে)
        //
        //   createPurchase = true হলে:
        //     → Purchase তৈরি হয়, Purchase-ই stock ও ledger manage করে
        //     → AddDeliveryInternal stock/ledger skip করে (PurchaseId set থাকে)
        //
        //   createPurchase = false হলে:
        //     → AddDeliveryInternal নিজেই stock ও ledger handle করে
        // =====================================================================

        // ---------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------
        public bool CreatePreOrder(PreOrder preOrder)
        {
            if (preOrder == null || preOrder.PreOrderDetails == null || !preOrder.PreOrderDetails.Any())
                return false;

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var initialPayment = preOrder.TotalPaid;
                preOrder.TotalPaid = 0;
                preOrder.Status = PreOrderStatus.Pending;

                foreach (var detail in preOrder.PreOrderDetails)
                {
                    detail.DeliveredQuantity = 0;
                    detail.CancelledQuantity = 0;
                }

                _dbContext.PreOrders.Add(preOrder);
                _dbContext.SaveChanges();

                // Return empty cylinder company ke diye deya hoy => amader empty stock kombe
                foreach (var detail in preOrder.PreOrderDetails)
                {
                    ApplyReturnEmptyStock(detail.ProductId, detail.ReturnQuantity ?? 0);
                }
                _dbContext.SaveChanges();

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

                var hasPayment = _dbContext.PreOrderPayments
                    .Any(p => p.PreOrderId == existing.Id && !p.IsDeleted);

                if (hasPayment && existing.SupplierId != preOrder.SupplierId)
                    throw new InvalidOperationException("Payment deya thakle Supplier change kora jabe na.");

                // Step 1: purono empty return effect reverse (detail mutate korar age)
                foreach (var old in existing.PreOrderDetails.ToList())
                {
                    ApplyReturnEmptyStock(old.ProductId, -(old.ReturnQuantity ?? 0));
                }

                existing.SupplierId = preOrder.SupplierId;
                existing.OrderDate = preOrder.OrderDate;
                existing.ExpectedDeliveryDate = preOrder.ExpectedDeliveryDate;
                existing.Comments = preOrder.Comments;
                existing.PaymentMethodId = preOrder.PaymentMethodId;
                existing.SubTotal = preOrder.SubTotal;
                existing.Discount = preOrder.Discount;
                existing.TotalAmount = preOrder.TotalAmount;

                var incomingIds = preOrder.PreOrderDetails
                    .Where(d => d.Id > 0).Select(d => d.Id).ToList();

                var removed = existing.PreOrderDetails
                    .Where(d => !incomingIds.Contains(d.Id)).ToList();

                foreach (var r in removed)
                {
                    if (r.CancelledQuantity > 0 ||
                        _dbContext.PreOrderDeliveryDetails.Any(x => x.PreOrderDetailId == r.Id))
                        throw new InvalidOperationException(
                            "Delivery/Cancel hoye gese emon item remove kora jabe na.");

                    _dbContext.PreOrderDetails.Remove(r);
                }

                foreach (var incoming in preOrder.PreOrderDetails)
                {
                    var current = existing.PreOrderDetails
                        .FirstOrDefault(d => d.Id == incoming.Id && incoming.Id > 0);

                    if (current == null)
                    {
                        existing.PreOrderDetails.Add(new PreOrderDetail
                        {
                            ProductId = incoming.ProductId,
                            OrderedQuantity = incoming.OrderedQuantity,
                            ReturnQuantity = incoming.ReturnQuantity ?? 0,
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
                        current.ReturnQuantity = incoming.ReturnQuantity ?? 0;
                        current.CylinderUnitPrice = incoming.CylinderUnitPrice;
                        current.GasUnitPrice = incoming.GasUnitPrice;
                        current.Discount = incoming.Discount;
                        current.Price = incoming.Price;
                    }
                }

                // Step 2: notun empty return effect apply (removed row gulo baad diye)
                var removedSet = removed.ToHashSet();
                foreach (var detail in existing.PreOrderDetails.ToList().Where(d => !removedSet.Contains(d)))
                {
                    ApplyReturnEmptyStock(detail.ProductId, detail.ReturnQuantity ?? 0);
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
                if (preOrder == null) return false;
                if (preOrder.IsDeleted) return true;

                var hasActiveDelivery = _dbContext.PreOrderDeliveries
                    .Any(d => d.PreOrderId == id && !d.IsDeleted);

                if (hasActiveDelivery)
                    throw new InvalidOperationException(
                        "Delivery thakle order delete kora jabe na. Age delivery delete koro.");

                var payments = _dbContext.PreOrderPayments
                    .Where(p => p.PreOrderId == id && !p.IsDeleted).ToList();

                foreach (var payment in payments)
                    DeletePaymentCore(preOrder, payment);

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
        // CANCEL PENDING QUANTITY
        // ---------------------------------------------------------------
        public bool CancelPendingQuantity(int preOrderDetailId, int quantity)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var detail = _dbContext.PreOrderDetails
                    .FirstOrDefault(d => d.Id == preOrderDetailId);

                if (detail == null) return false;

                if (quantity <= 0 || quantity > detail.PendingQuantity)
                    throw new InvalidOperationException(
                        $"Cancel quantity 1 theke {detail.PendingQuantity} er moddhe hote hobe.");

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
            if (payment == null || payment.Amount <= 0) return false;

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var preOrder = _dbContext.PreOrders
                    .FirstOrDefault(o => o.Id == payment.PreOrderId && !o.IsDeleted);

                if (preOrder == null) return false;

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
                var payment = _dbContext.PreOrderPayments
                    .FirstOrDefault(p => p.Id == paymentId);

                if (payment == null) return false;
                if (payment.IsDeleted) return true;

                var preOrder = _dbContext.PreOrders
                    .First(o => o.Id == payment.PreOrderId);

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
        // DELIVERY — সাধারণ (backward compatible)
        // ---------------------------------------------------------------
        public bool AddDelivery(PreOrderDelivery delivery)
        {
            if (delivery == null ||
                delivery.PreOrderDeliveryDetails == null ||
                !delivery.PreOrderDeliveryDetails.Any())
                return false;

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var preOrder = _dbContext.PreOrders
                    .Include(o => o.PreOrderDetails)
                    .FirstOrDefault(o => o.Id == delivery.PreOrderId && !o.IsDeleted);

                if (preOrder == null) return false;

                if (preOrder.Status == PreOrderStatus.Cancelled)
                    throw new InvalidOperationException("Cancelled pre-order e delivery neya jabe na.");

                // PurchaseId নেই → AddDeliveryInternal নিজেই stock ও ledger handle করবে
                AddDeliveryInternal(preOrder, delivery);
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
        // DELIVERY — Purchase auto-create সহ (নতুন)
        // ---------------------------------------------------------------
        public bool AddDeliveryWithPurchase(
            PreOrderDelivery delivery,
            bool createPurchase,
            DeliveryPurchaseOptions options = null)
        {
            if (delivery == null ||
                delivery.PreOrderDeliveryDetails == null ||
                !delivery.PreOrderDeliveryDetails.Any())
                return false;

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var preOrder = _dbContext.PreOrders
                    .Include(o => o.PreOrderDetails)
                    .FirstOrDefault(o => o.Id == delivery.PreOrderId && !o.IsDeleted);

                if (preOrder == null) return false;

                if (preOrder.Status == PreOrderStatus.Cancelled)
                    throw new InvalidOperationException("Cancelled pre-order e delivery neya jabe na.");

                if (createPurchase)
                {
                    options ??= new DeliveryPurchaseOptions();

                    // ── Step 1: Purchase object তৈরি ─────────────────────────
                    var purchase = BuildPurchaseFromDelivery(preOrder, delivery, options);

                    // ── Step 2: Purchase DB তে সেভ করো (Id পাওয়ার জন্য) ─────
                    _dbContext.Purchases.Add(purchase);
                    _dbContext.SaveChanges();

                    // ── Step 3: Stock আপডেট (Purchase এর জন্য) ───────────────
                    foreach (var item in delivery.PreOrderDeliveryDetails)
                    {
                        var od = preOrder.PreOrderDetails
                            .First(d => d.Id == item.PreOrderDetailId);

                        var stock = _dbContext.ProductStocks
                            .FirstOrDefault(x => x.ProductId == od.ProductId);

                        if (stock != null)
                            stock.LoadedQty += item.Quantity;
                    }
                    _dbContext.SaveChanges();

                    // ── Step 4: Supplier Ledger (Purchase এর জন্য) ────────────
                    double dueBefore = GetLastSupplierDue(preOrder.SupplierId);

                    var supplierHistory = new SupplierPaymentHistory
                    {
                        SupplierId = preOrder.SupplierId,
                        PurchaseId = purchase.Id,
                        PaymentDate = delivery.DeliveryDate,
                        PaymentMethodId = options.PaymentMethodId,
                        Comments = options.Comments
                            ?? $"Auto-purchase from Pre-order {preOrder.OrderNumber}",
                        Number = options.InvoiceNumber ?? delivery.ChallanNumber,
                        TotalAmountThisPurchase = purchase.TotalAmount,
                        AmountPaid = options.AmountPaid,
                        TotalDueBeforePayment = dueBefore,
                        TotalDueAfterPayment = dueBefore + purchase.TotalAmount - options.AmountPaid
                    };
                    _dbContext.SupplierPaymentHistories.Add(supplierHistory);
                    _dbContext.SaveChanges();

                    // ── Step 5: Cash/Bank transaction (AmountPaid > 0 হলে) ────
                    if (options.AmountPaid > 0)
                    {
                        var existBalance = _dbContext.TransactionHistories
                            .AsNoTracking()
                            .Where(x => !x.IsDeleted)
                            .OrderByDescending(x => x.Id)
                            .FirstOrDefault()?.CurrentBalance ?? 0;

                        _dbContext.TransactionHistories.Add(new TransactionHistory
                        {
                            BalanceIn = 0,
                            BalanceOut = options.AmountPaid,
                            CurrentBalance = existBalance - options.AmountPaid,
                            Date = delivery.DeliveryDate,
                            PurchaseId = purchase.Id,
                            Resone = $"Auto-purchase payment ({preOrder.OrderNumber})"
                        });
                        _dbContext.SaveChanges();
                    }

                    // ── Step 6: Delivery এর সাথে Purchase link করো ───────────
                    delivery.PurchaseId = purchase.Id;

                    // ── Step 7: Delivery সেভ করো ─────────────────────────────
                    // PurchaseId set থাকায় AddDeliveryInternal stock/ledger skip করবে ✅
                    AddDeliveryInternal(preOrder, delivery);
                    _dbContext.SaveChanges();

                    // ── Step 8: Purchase এ Delivery back-link ─────────────────
                    purchase.PreOrderDeliveryId = delivery.Id;
                    _dbContext.SaveChanges();
                }
                else
                {
                    // Purchase ছাড়া — সাধারণ path
                    // AddDeliveryInternal নিজেই stock ও ledger handle করবে
                    AddDeliveryInternal(preOrder, delivery);
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

        // ---------------------------------------------------------------
        // DELETE DELIVERY
        // ---------------------------------------------------------------
        public bool DeleteDelivery(int deliveryId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var delivery = _dbContext.PreOrderDeliveries
                    .Include(d => d.PreOrderDeliveryDetails)
                    .FirstOrDefault(d => d.Id == deliveryId);

                if (delivery == null) return false;
                if (delivery.IsDeleted) return true;

                var preOrder = _dbContext.PreOrders
                    .Include(o => o.PreOrderDetails)
                    .First(o => o.Id == delivery.PreOrderId);

                delivery.IsDeleted = true;

                foreach (var item in delivery.PreOrderDeliveryDetails)
                {
                    var detail = preOrder.PreOrderDetails
                        .FirstOrDefault(d => d.Id == item.PreOrderDetailId);

                    if (detail != null)
                        detail.DeliveredQuantity = Math.Max(0, detail.DeliveredQuantity - item.Quantity);

                    // Purchase নেই হলে এখানেই stock reverse করো
                    if (!delivery.PurchaseId.HasValue)
                    {
                        var stock = _dbContext.ProductStocks
                            .FirstOrDefault(x => x.ProductId == item.ProductId);
                        if (stock != null)
                            stock.LoadedQty -= item.Quantity;
                    }
                }

                UpdatePreOrderStatus(preOrder);
                _dbContext.SaveChanges();

                // ── Linked Purchase থাকলে সেটাও soft-delete করো ──────────────
                if (delivery.PurchaseId.HasValue)
                {
                    var linkedPurchase = _dbContext.Purchases
                        .Include(p => p.PurchaseDetails)
                        .FirstOrDefault(p => p.Id == delivery.PurchaseId.Value);

                    if (linkedPurchase != null && !linkedPurchase.IsDeleted)
                    {
                        // Stock reverse (Purchase details থেকে)
                        foreach (var pd in linkedPurchase.PurchaseDetails)
                        {
                            var stock = _dbContext.ProductStocks
                                .FirstOrDefault(x => x.ProductId == pd.ProductId);
                            if (stock != null)
                                stock.LoadedQty -= pd.Quantity;
                        }

                        // Supplier Ledger reverse
                        var ledger = _dbContext.SupplierPaymentHistories
                            .FirstOrDefault(h => h.PurchaseId == linkedPurchase.Id && !h.IsDeleted);

                        if (ledger != null)
                        {
                            ledger.IsDeleted = true;
                            RecalculateSupplierPaymentHistories(
                                ledger.SupplierId,
                                ledger.Id,
                                (double)(ledger.TotalAmountThisPurchase - ledger.AmountPaid));
                        }

                        // TransactionHistory reverse
                        var txn = _dbContext.TransactionHistories
                            .FirstOrDefault(t => t.PurchaseId == linkedPurchase.Id && !t.IsDeleted);

                        if (txn != null)
                        {
                            if (txn.BalanceOut.HasValue && txn.BalanceOut.Value > 0)
                                BalanceInTransactionHistories(txn.Id, txn.BalanceOut.Value);

                            txn.IsDeleted = true;
                            _dbContext.Update(txn);
                        }

                        linkedPurchase.IsDeleted = true;
                        _dbContext.SaveChanges();
                    }
                }
                else
                {
                    // সরাসরি delivery (Purchase ছাড়া) এর ledger reverse
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
                    // Linked Purchase info দেখানোর জন্য
                    .Include(o => o.PreOrderDeliveries.Where(d => !d.IsDeleted))
                        .ThenInclude(d => d.Purchase)
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
                        && (o.Status == PreOrderStatus.Pending ||
                            o.Status == PreOrderStatus.PartiallyDelivered));

                if (supplierId.HasValue)
                    query = query.Where(o => o.SupplierId == supplierId.Value);

                return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // ---------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------

        /// <summary>
        /// Delivery এর core logic (transaction ছাড়া — caller এর transaction আছে)।
        /// - সব সময়: quantity validate, DeliveredQuantity update, delivery row add, status update।
        /// - PurchaseId NOT set হলে: stock ও supplier ledger ও update।
        /// - PurchaseId set হলে: stock/ledger skip (Purchase ই করেছে)। ✅
        /// </summary>
        private void AddDeliveryInternal(PreOrder preOrder, PreOrderDelivery delivery)
        {
            double factor = preOrder.SubTotal > 0
                ? preOrder.TotalAmount / preOrder.SubTotal
                : 1;

            double deliveredValue = 0;

            foreach (var item in delivery.PreOrderDeliveryDetails)
            {
                var detail = preOrder.PreOrderDetails
                    .FirstOrDefault(d => d.Id == item.PreOrderDetailId);

                if (detail == null)
                    throw new InvalidOperationException("Delivery item ei order-er na.");

                if (item.Quantity <= 0)
                    throw new InvalidOperationException("Delivery quantity 0 er beshi hote hobe.");

                if (item.Quantity > detail.PendingQuantity)
                    throw new InvalidOperationException(
                        $"Pending quantity ({detail.PendingQuantity}) er beshi delivery neya jabe na.");

                item.ProductId = detail.ProductId;
                detail.DeliveredQuantity += item.Quantity;

                if (detail.OrderedQuantity > 0)
                    deliveredValue += detail.Price / detail.OrderedQuantity * item.Quantity * factor;

                // Stock: শুধু Purchase না থাকলে এখানে করো
                if (!delivery.PurchaseId.HasValue)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(x => x.ProductId == item.ProductId);
                    if (stock != null)
                        stock.LoadedQty += item.Quantity;
                }
            }

            deliveredValue = Math.Round(deliveredValue, 2);

            _dbContext.PreOrderDeliveries.Add(delivery);
            UpdatePreOrderStatus(preOrder);
            _dbContext.SaveChanges(); // delivery.Id পাওয়ার জন্য

            // Supplier Ledger: শুধু Purchase না থাকলে এখানে করো
            if (!delivery.PurchaseId.HasValue)
            {
                double dueBefore = GetLastSupplierDue(preOrder.SupplierId);

                _dbContext.SupplierPaymentHistories.Add(new SupplierPaymentHistory
                {
                    SupplierId = preOrder.SupplierId,
                    PreOrderDeliveryId = delivery.Id,
                    PaymentDate = delivery.DeliveryDate,
                    Comments = delivery.Comments
                        ?? $"Pre-order delivery ({preOrder.OrderNumber})",
                    Number = delivery.ChallanNumber,
                    TotalAmountThisPurchase = deliveredValue,
                    AmountPaid = 0,
                    TotalDueBeforePayment = dueBefore,
                    TotalDueAfterPayment = dueBefore + deliveredValue
                });
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Delivered items থেকে Purchase object তৈরি করো (persist করে না)।
        /// IsFromPreOrder = true সেট করা হয়।
        /// </summary>
        private Purchase BuildPurchaseFromDelivery(
            PreOrder preOrder,
            PreOrderDelivery delivery,
            DeliveryPurchaseOptions options)
        {
            double factor = preOrder.SubTotal > 0
                ? preOrder.TotalAmount / preOrder.SubTotal
                : 1;

            var details = delivery.PreOrderDeliveryDetails.Select(item =>
            {
                var od = preOrder.PreOrderDetails
                    .First(d => d.Id == item.PreOrderDetailId);

                double lineTotal = od.OrderedQuantity > 0
                    ? Math.Round(od.Price / od.OrderedQuantity * item.Quantity * factor, 2)
                    : 0;

                return new PurchaseDetail
                {
                    ProductId = od.ProductId,
                    Quantity = item.Quantity,
                    ReturnQuantity = od.ReturnQuantity,
                    CylinderUnitPrice = od.CylinderUnitPrice,
                    GasUnitPrice = od.GasUnitPrice,
                    Discount = 0,
                    Price = lineTotal
                };
            }).ToList();

            double totalAmount = details.Sum(d => d.Price);

            return new Purchase
            {
                SupplierId = preOrder.SupplierId,
                Date = delivery.DeliveryDate,
                Number = string.IsNullOrWhiteSpace(options.InvoiceNumber)
                    ? delivery.ChallanNumber
                    : options.InvoiceNumber,
                PaymentMethodId = options.PaymentMethodId,
                Comments = string.IsNullOrWhiteSpace(options.Comments)
                    ? $"Auto from Pre-order {preOrder.OrderNumber}"
                    : options.Comments,
                TotalAmount = totalAmount,
                TotalPay = options.AmountPaid,
                TotalDue = totalAmount - options.AmountPaid,
                Discount = 0,
                DeliveryCharge = 0,
                Adjustment = 0,
                DamageProductDueAdjustment = 0,
                IsDeleted = false,
                IsFromPreOrder = true,       // ← Delete block এর জন্য
                PurchaseDetails = details
                // PreOrderDeliveryId → delivery save এর পরে set হবে
            };
        }

        // Transaction ছাড়া payment save (CreatePreOrder ও AddPayment দুটোই ব্যবহার করে)
        private void AddPaymentCore(PreOrder preOrder, PreOrderPayment payment)
        {
            payment.PreOrderId = preOrder.Id;
            payment.IsDeleted = false;

            _dbContext.PreOrderPayments.Add(payment);
            preOrder.TotalPaid += payment.Amount;
            _dbContext.SaveChanges();

            double dueBefore = GetLastSupplierDue(preOrder.SupplierId);

            _dbContext.SupplierPaymentHistories.Add(new SupplierPaymentHistory
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
            });
            _dbContext.SaveChanges();

            var existCurrentBalance = _dbContext.TransactionHistories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault()?.CurrentBalance ?? 0;

            _dbContext.Add(new TransactionHistory
            {
                BalanceIn = 0,
                BalanceOut = payment.Amount,
                CurrentBalance = existCurrentBalance - payment.Amount,
                Date = payment.Date,
                PreOrderPaymentId = payment.Id,
                Resone = $"Pre-order payment ({preOrder.OrderNumber})."
            });
            _dbContext.SaveChanges();
        }

        // Transaction ছাড়া payment reverse
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
                    BalanceInTransactionHistories(txn.Id, txn.BalanceOut.Value);

                txn.IsDeleted = true;
                _dbContext.Update(txn);
            }

            _dbContext.SaveChanges();
        }

        // Order এর status recalculate করো
        private void UpdatePreOrderStatus(PreOrder preOrder)
        {
            var details = preOrder.PreOrderDetails?.ToList();
            if (details == null || !details.Any()) return;

            var totalDelivered = details.Sum(d => d.DeliveredQuantity);
            var totalPending = details.Sum(d => d.PendingQuantity);

            if (totalPending <= 0)
                preOrder.Status = totalDelivered > 0
                    ? PreOrderStatus.Delivered
                    : PreOrderStatus.Cancelled;
            else
                preOrder.Status = totalDelivered > 0
                    ? PreOrderStatus.PartiallyDelivered
                    : PreOrderStatus.Pending;
        }

        private double GetLastSupplierDue(int supplierId)
        {
            return _dbContext.SupplierPaymentHistories
                .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .Select(p => (double?)p.TotalDueAfterPayment)
                .FirstOrDefault() ?? 0;
        }

        // পরের সব ledger row এর due সামঞ্জস্য করো
        private void RecalculateSupplierPaymentHistories(int supplierId, int id, double amount)
        {
            if (amount == 0) return;

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

        private void ApplyReturnEmptyStock(int productId, int returnQuantity)
        {
            if (returnQuantity == 0)
                return;

            var stock = _dbContext.ProductStocks
                .FirstOrDefault(x => x.ProductId == productId);

            if (stock == null)
                return;

            // Company ke empty ferot deya => amader empty stock kombe (Purchase er same)
            stock.EmptyQty -= returnQuantity;
        }
    }
}