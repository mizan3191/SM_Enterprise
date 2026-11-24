namespace DEALER.DataAccess
{
    public class CylinderExchangeManager : BaseDataManager, ICylinderExchange
    {
        public CylinderExchangeManager(DEALERContext model) : base(model)
        {
        }

        // ============================================================
        // CREATE
        // ============================================================
        public void CreateCylinderExchange(CylinderExchange exchange)
        {
            if (exchange == null || exchange.ExchangeDetails == null || !exchange.ExchangeDetails.Any())
                throw new Exception("Exchange details required.");

            if (string.IsNullOrEmpty(exchange.ExchangeType))
                throw new Exception("Exchange type is required.");

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // Step 1: Detail price + discount + stock update
                foreach (var detail in exchange.ExchangeDetails)
                {
                    var product = _dbContext.Products
                        .Include(p => p.CurrentPrice)
                        .FirstOrDefault(p => p.Id == detail.ProductId);

                    if (product == null)
                        throw new Exception($"Product {detail.ProductId} not found");

                    if (detail.CylinderUnitPrice <= 0)
                        detail.CylinderUnitPrice = product.CurrentPrice?.CylinderBuyingPrice ?? 0;

                    // Product-wise discount amount calculate
                    var subTotal = detail.Quantity * detail.CylinderUnitPrice;
                    detail.DiscountAmount = subTotal * (detail.DiscountPercent / 100);

                    // Stock update
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock == null)
                    {
                        stock = new ProductStock { ProductId = detail.ProductId, LoadedQty = 0, EmptyQty = 0 };
                        _dbContext.ProductStocks.Add(stock);
                    }

                    if (exchange.ExchangeType == "Give")
                        stock.EmptyQty -= detail.Quantity;
                    else
                        stock.EmptyQty += detail.Quantity;
                }

                // Step 2: SubTotal (discount er age)
                exchange.SubTotal = exchange.ExchangeDetails
                    .Sum(d => d.Quantity * d.CylinderUnitPrice);

                // Step 3: Total = SubTotal - ProductDiscount - ExchangeDiscount
                var productDiscount = exchange.ExchangeDetails.Sum(d => d.DiscountAmount);
                exchange.TotalAmount = exchange.SubTotal - productDiscount - exchange.ExchangeDiscount;

                // Step 4: Due
                exchange.TotalDue = exchange.TotalAmount - exchange.TotalPay;

                // Step 5: Save
                _dbContext.CylinderExchanges.Add(exchange);
                _dbContext.SaveChanges();

                // Step 6: Payment history + Transaction history
                if (exchange.TotalPay > 0)
                {
                    var payment = new ExchangePaymentHistory
                    {
                        CylinderExchangeId = exchange.Id,
                        DealerPointId = exchange.DealerPointId,   // <-- missing chilo
                        Amount = exchange.TotalPay,
                        Direction = exchange.ExchangeType,
                        PaymentMethodId = exchange.PaymentMethodId,
                        Date = exchange.Date,
                        Comments = exchange.Comments,
                        IsDeleted = false
                    };
                    _dbContext.ExchangePaymentHistories.Add(payment);
                    _dbContext.SaveChanges(); // payment.Id lagbe

                    var existCurrentBalance = _dbContext.TransactionHistories
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault()?.CurrentBalance ?? 0;

                    // Give    = dealer amake taka dibe  -> cash in  (BalanceIn)
                    // Receive = ami dealer ke taka dibo  -> cash out (BalanceOut)
                    bool isGive = payment.Direction == "Give";
                    double balanceIn = isGive ? payment.Amount : 0;
                    double balanceOut = isGive ? 0 : payment.Amount;

                    _dbContext.TransactionHistories.Add(new TransactionHistory
                    {
                        BalanceIn = balanceIn,
                        BalanceOut = balanceOut,
                        CurrentBalance = existCurrentBalance + balanceIn - balanceOut,
                        Date = payment.Date,
                        ExchangePaymentHistoryId = payment.Id,
                        Resone = $"Empty Cylinder ({payment.Direction})"
                    });
                }

                _dbContext.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public void UpdateCylinderExchange(CylinderExchange exchange)
        {
            if (exchange == null ||
                exchange.ExchangeDetails == null ||
                !exchange.ExchangeDetails.Any())
            {
                throw new Exception("Exchange details required.");
            }

            if (string.IsNullOrEmpty(exchange.ExchangeType))
                throw new Exception("Exchange type is required.");

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingExchange = _dbContext.CylinderExchanges
                    .Include(e => e.ExchangeDetails)
                    .Include(e => e.ExchangePaymentHistories)
                    .FirstOrDefault(e => e.Id == exchange.Id);

                if (existingExchange == null)
                    throw new Exception("Exchange not found.");

                // ============================================
                // Step 1: Reverse old stock changes (header ExchangeType use koro)
                // ============================================
                foreach (var oldDetail in existingExchange.ExchangeDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == oldDetail.ProductId);

                    if (stock != null)
                    {
                        if (existingExchange.ExchangeType == "Give")
                            stock.EmptyQty += oldDetail.Quantity;   // reverse
                        else
                            stock.EmptyQty -= oldDetail.Quantity;   // reverse
                    }
                }

                // Remove old details
                _dbContext.CylinderExchangeDetails.RemoveRange(existingExchange.ExchangeDetails);
                _dbContext.SaveChanges();

                // ============================================
                // Step 2: Apply new stock changes (+ discount recalc)
                // ============================================
                foreach (var detail in exchange.ExchangeDetails)
                {
                    var product = _dbContext.Products
                        .Include(p => p.CurrentPrice)
                        .FirstOrDefault(p => p.Id == detail.ProductId);

                    if (product == null)
                        throw new Exception($"Product {detail.ProductId} not found");

                    if (detail.CylinderUnitPrice <= 0)
                        detail.CylinderUnitPrice = product.CurrentPrice?.CylinderBuyingPrice ?? 0;

                    // Create er moto discount recalculate (age missing chilo)
                    var subTotal = detail.Quantity * detail.CylinderUnitPrice;
                    detail.DiscountAmount = subTotal * (detail.DiscountPercent / 100);

                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock == null)
                    {
                        stock = new ProductStock { ProductId = detail.ProductId, LoadedQty = 0, EmptyQty = 0 };
                        _dbContext.ProductStocks.Add(stock);
                    }

                    if (exchange.ExchangeType == "Give")
                        stock.EmptyQty -= detail.Quantity;
                    else
                        stock.EmptyQty += detail.Quantity;
                }

                // Step 3: Update header
                existingExchange.DealerPointId = exchange.DealerPointId;
                existingExchange.ExchangeType = exchange.ExchangeType;
                existingExchange.PaymentMethodId = exchange.PaymentMethodId;
                existingExchange.Date = exchange.Date;
                existingExchange.Comments = exchange.Comments;
                existingExchange.ExchangeDiscount = exchange.ExchangeDiscount;   // <-- missing chilo
                existingExchange.TotalPay = exchange.TotalPay;

                existingExchange.SubTotal = exchange.ExchangeDetails
                    .Sum(d => d.Quantity * d.CylinderUnitPrice);

                var productDiscount = exchange.ExchangeDetails.Sum(d => d.DiscountAmount);
                existingExchange.TotalAmount = existingExchange.SubTotal - productDiscount - exchange.ExchangeDiscount;
                existingExchange.TotalDue = existingExchange.TotalAmount - exchange.TotalPay;

                // Step 4: Add new details
                foreach (var detail in exchange.ExchangeDetails)
                {
                    detail.Id = 0; // Notun detail hishebe add
                    detail.CylinderExchangeId = exchange.Id;
                    existingExchange.ExchangeDetails.Add(detail);
                }

                _dbContext.SaveChanges();

                // ============================================
                // Step 5: Payment history + Transaction history sync
                // ============================================
                var existingPayment = existingExchange.ExchangePaymentHistories
                    .FirstOrDefault(p => !p.IsDeleted);

                // Ei exchange er sob payment id (soft-deleted o include) diye linked transaction row khuji
                var exchangePaymentIds = existingExchange.ExchangePaymentHistories.Select(p => p.Id).ToList();
                var linkedTransaction = _dbContext.TransactionHistories
                    .FirstOrDefault(x => x.ExchangePaymentHistoryId != null
                                      && exchangePaymentIds.Contains(x.ExchangePaymentHistoryId.Value));

                ExchangePaymentHistory payment = existingPayment;

                if (exchange.TotalPay > 0)
                {
                    if (existingPayment != null)
                    {
                        existingPayment.DealerPointId = exchange.DealerPointId;
                        existingPayment.Amount = exchange.TotalPay;
                        existingPayment.Direction = exchange.ExchangeType;
                        existingPayment.PaymentMethodId = exchange.PaymentMethodId;
                        existingPayment.Date = exchange.Date;
                        existingPayment.Comments = exchange.Comments;
                    }
                    else
                    {
                        payment = new ExchangePaymentHistory
                        {
                            CylinderExchangeId = exchange.Id,
                            DealerPointId = exchange.DealerPointId,
                            Amount = exchange.TotalPay,
                            Direction = exchange.ExchangeType,
                            PaymentMethodId = exchange.PaymentMethodId,
                            Date = exchange.Date,
                            Comments = exchange.Comments,
                            IsDeleted = false
                        };
                        _dbContext.ExchangePaymentHistories.Add(payment);
                    }

                    _dbContext.SaveChanges(); // payment.Id confirm

                    // Nijer row bad diye ager balance
                    var balanceQuery = _dbContext.TransactionHistories
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted);

                    if (linkedTransaction != null)
                    {
                        int linkedId = linkedTransaction.Id;
                        balanceQuery = balanceQuery.Where(x => x.Id != linkedId);
                    }

                    var existCurrentBalance = balanceQuery
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault()?.CurrentBalance ?? 0;

                    bool isGive = payment.Direction == "Give";
                    double balanceIn = isGive ? payment.Amount : 0;
                    double balanceOut = isGive ? 0 : payment.Amount;

                    if (linkedTransaction != null)
                    {
                        // Age er row reuse (duplicate hobe na)
                        linkedTransaction.IsDeleted = false;
                        linkedTransaction.ExchangePaymentHistoryId = payment.Id;
                        linkedTransaction.BalanceIn = balanceIn;
                        linkedTransaction.BalanceOut = balanceOut;
                        linkedTransaction.CurrentBalance = existCurrentBalance + balanceIn - balanceOut;
                        linkedTransaction.Date = payment.Date;
                        linkedTransaction.Resone = $"Empty Cylinder Exchange ({payment.Direction}) - Dealer #{exchange.DealerPointId}";
                    }
                    else
                    {
                        _dbContext.TransactionHistories.Add(new TransactionHistory
                        {
                            BalanceIn = balanceIn,
                            BalanceOut = balanceOut,
                            CurrentBalance = existCurrentBalance + balanceIn - balanceOut,
                            Date = payment.Date,
                            ExchangePaymentHistoryId = payment.Id,
                            Resone = $"Empty Cylinder Exchange ({payment.Direction})"
                        });
                    }
                }
                else
                {
                    // TotalPay 0 hole payment + transaction dutoi soft delete
                    if (existingPayment != null)
                        existingPayment.IsDeleted = true;

                    if (linkedTransaction != null)
                        linkedTransaction.IsDeleted = true;
                }

                _dbContext.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        // ============================================================
        // DELETE (Soft Delete + Reverse Stock)
        // ============================================================
        public void DeleteCylinderExchange(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var exchange = _dbContext.CylinderExchanges
                    .Include(e => e.ExchangeDetails)
                    .Include(e => e.ExchangePaymentHistories)
                    .FirstOrDefault(e => e.Id == id);

                if (exchange == null)
                    throw new Exception("Exchange not found.");

                if (exchange.IsDeleted)
                    return;

                // ============================================
                // Reverse stock changes (header ExchangeType use koro)
                // ============================================
                foreach (var detail in exchange.ExchangeDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock != null)
                    {
                        if (exchange.ExchangeType == "Give")
                            stock.EmptyQty += detail.Quantity;   // reverse
                        else
                            stock.EmptyQty -= detail.Quantity;   // reverse
                    }
                }

                // Soft delete exchange
                exchange.IsDeleted = true;

                var paymentIds = exchange.ExchangePaymentHistories.Select(p => p.Id).ToList();

                // Soft delete payments
                foreach (var payment in exchange.ExchangePaymentHistories)
                    payment.IsDeleted = true;

                // Linked transaction history soft delete (transaction page e bhut entry theke jabe na)
                if (paymentIds.Any())
                {
                    var linkedTransactions = _dbContext.TransactionHistories
                        .Where(x => !x.IsDeleted
                                 && x.ExchangePaymentHistoryId != null
                                 && paymentIds.Contains(x.ExchangePaymentHistoryId.Value))
                        .ToList();

                    foreach (var t in linkedTransactions)
                        t.IsDeleted = true;
                }

                _dbContext.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public CylinderExchange GetCylinderExchangeById(int id)
        {
            try
            {
                return _dbContext.CylinderExchanges
                    .Include(e => e.DealerPoint)
                    .Include(e => e.PaymentMethod)
                    .Include(e => e.ExchangeDetails)
                        .ThenInclude(d => d.Product)
                    .Include(e => e.ExchangePaymentHistories)
                    .FirstOrDefault(e => e.Id == id && !e.IsDeleted);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ============================================================
        // GET ALL
        // ============================================================
        public IEnumerable<CylinderExchange> GetAllCylinderExchange()
        {
            try
            {
                return _dbContext.CylinderExchanges
                    .Include(e => e.DealerPoint)
                    .Include(e => e.PaymentMethod)
                    .Include(e => e.ExchangeDetails)
                        .ThenInclude(d => d.Product)
                    .Include(e => e.ExchangePaymentHistories)
                    .Where(e => !e.IsDeleted)
                    .OrderByDescending(e => e.Date)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<CylinderExchange>();
            }
        }

        // ============================================================
        // GET BY SUPPLIER (single latest)
        // ============================================================
        public CylinderExchange GetCylinderExchangeBySupplierId(int dealerId)
        {
            try
            {
                return _dbContext.CylinderExchanges
                    .Include(e => e.DealerPoint)
                    .Include(e => e.PaymentMethod)
                    .Include(e => e.ExchangeDetails)
                        .ThenInclude(d => d.Product)
                    .Include(e => e.ExchangePaymentHistories)
                    .Where(e => e.DealerPointId == dealerId && !e.IsDeleted)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<CylinderExchangeDetailsDTO>> GetCylinderExchangeDetailsByIdAsync(int exchangeId)
        {
            try
            {
                var query = from ce in _dbContext.CylinderExchanges
                            join ced in _dbContext.CylinderExchangeDetails on ce.Id equals ced.CylinderExchangeId
                            join p in _dbContext.Products on ced.ProductId equals p.Id
                            join s in _dbContext.DealerPoints on ce.DealerPointId equals s.Id into supplierJoin
                            from s in supplierJoin.DefaultIfEmpty()
                            where ce.Id == exchangeId && !ce.IsDeleted
                            select new CylinderExchangeDetailsDTO
                            {
                                CylinderExchangeId = ce.Id,
                                DealerPointName = s != null ? s.Name : "",
                                ExchangeType = ce.ExchangeType,
                                ExchangeDate = ce.Date,

                                ProductId = ced.ProductId,
                                ProductName = p.DisplayNameSize,
                                CompanyName = p.Supplier != null ? p.Supplier.Name : "",

                                Quantity = ced.Quantity,
                                CylinderUnitPrice = ced.CylinderUnitPrice,
                                DiscountPercent = ced.DiscountPercent,
                                DiscountAmount = ced.DiscountAmount,
                                TotalPrice = ced.TotalPrice,

                                SubTotal = ce.SubTotal,
                                ExchangeDiscount = ce.ExchangeDiscount,
                                TotalAmount = ce.TotalAmount,
                                TotalPay = ce.TotalPay,
                                TotalDue = ce.TotalDue,
                                Comments = ce.Comments
                            };

                return await query.ToListAsync();
            }
            catch (Exception)
            {
                return new List<CylinderExchangeDetailsDTO>();
            }
        }

        public IEnumerable<DealerPointDueDTO> GetDealerPointDueSummary()
        {
            // Exchange header theke: Give = +, Receive = −
            var exchangeSums = _dbContext.CylinderExchanges
                .Where(x => !x.IsDeleted && x.DealerPointId != null)
                .GroupBy(x => x.DealerPointId.Value)
                .Select(g => new
                {
                    DealerPointId = g.Key,
                    Net = g.Sum(e => (e.ExchangeType == "Give" ? 1 : -1)
                                     * (e.TotalAmount - e.TotalPay))
                })
                .ToDictionary(x => x.DealerPointId, x => x.Net);

            // Standalone payment (exchange sathe linked payment gulo TotalPay te already ache)
            var paymentSums = _dbContext.ExchangePaymentHistories
                .Where(x => !x.IsDeleted
                            && x.CylinderExchangeId == null
                            && x.DealerPointId != null)
                .GroupBy(x => x.DealerPointId.Value)
                .Select(g => new
                {
                    DealerPointId = g.Key,
                    Net = g.Sum(p => (p.Direction == "Give" ? 1 : -1) * p.Amount)
                })
                .ToDictionary(x => x.DealerPointId, x => x.Net);

            return _dbContext.DealerPoints
                .Where(d => !d.IsDisable)
                .Select(d => new DealerPointDueDTO
                {
                    Id = d.Id,
                    Name = d.Name,
                    Phone = d.Phone,
                    Address = d.Address
                })
                .ToList()
                .Select(d =>
                {
                    d.NetDue = exchangeSums.GetValueOrDefault(d.Id)
                             + paymentSums.GetValueOrDefault(d.Id);
                    return d;
                })
                .OrderByDescending(d => d.NetDue)
                .ToList();
        }

        public IEnumerable<Lov> GetDealerPointList()
        {
            return _dbContext.DealerPoints
                .Where(x => !x.IsDisable)
                .OrderBy(x => x.Name)
                .Select(x => new Lov { Id = x.Id, Name = x.Name })
                .ToList();
        }

        public int AddDealerPointPayment(ExchangePaymentHistory payment)
        {
            if (payment == null)
                throw new Exception("Payment required.");

            if (payment.Amount <= 0)
                throw new Exception("Payment amount must be greater than 0.");

            if (payment.Direction != "Give" && payment.Direction != "Receive")
                throw new Exception("Payment direction must be Give or Receive.");

            if (payment.DealerPointId == null)
                throw new Exception("Dealer point is required.");

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                payment.CylinderExchangeId = null;   // standalone dealer payment

                if (payment.Date == default)
                    payment.Date = DateTime.Now;

                _dbContext.ExchangePaymentHistories.Add(payment);
                _dbContext.SaveChanges();            // payment.Id lagbe

                // Standalone dealer payment -> TransactionHistory row
                var existCurrentBalance = _dbContext.TransactionHistories
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault()?.CurrentBalance ?? 0;

                // Dealer due formula onujayi (Give => +due, Receive => -due):
                // Give    = ami dealer ke taka dicchi   -> cash out (BalanceOut)
                // Receive = dealer amake taka dicche     -> cash in  (BalanceIn)
                bool isGive = payment.Direction == "Give";
                double balanceIn = isGive ? 0 : payment.Amount;
                double balanceOut = isGive ? payment.Amount : 0;

                _dbContext.TransactionHistories.Add(new TransactionHistory
                {
                    BalanceIn = balanceIn,
                    BalanceOut = balanceOut,
                    CurrentBalance = existCurrentBalance + balanceIn - balanceOut,
                    Date = payment.Date,
                    ExchangePaymentHistoryId = payment.Id,
                    Resone = $"Dealer Due Payment ({(isGive ? "Give" : "Receive")})"
                });

                _dbContext.SaveChanges();
                transaction.Commit();
                return payment.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}