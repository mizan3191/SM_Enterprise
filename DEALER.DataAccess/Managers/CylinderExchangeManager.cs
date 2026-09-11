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

                // Step 6: Payment history
                if (exchange.TotalPay > 0)
                {
                    var payment = new ExchangePaymentHistory
                    {
                        CylinderExchangeId = exchange.Id,
                        Amount = exchange.TotalPay,
                        Direction = exchange.ExchangeType,
                        PaymentMethodId = exchange.PaymentMethodId,
                        Date = exchange.Date,
                        Comments = exchange.Comments,
                        IsDeleted = false
                    };
                    _dbContext.ExchangePaymentHistories.Add(payment);
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
                        {
                            // Age Give korechi → EmptyQty komiyeche
                            // Ekhon reverse → EmptyQty BARABO
                            stock.EmptyQty += oldDetail.Quantity;
                        }
                        else // Receive
                        {
                            // Age Receive korechi → EmptyQty bariyeche
                            // Ekhon reverse → EmptyQty KOMABO
                            stock.EmptyQty -= oldDetail.Quantity;
                        }
                    }
                }

                // Remove old details
                _dbContext.CylinderExchangeDetails.RemoveRange(existingExchange.ExchangeDetails);
                _dbContext.SaveChanges();

                // ============================================
                // Step 2: Apply new stock changes
                // ============================================
                foreach (var detail in exchange.ExchangeDetails)
                {
                    var product = _dbContext.Products
                        .Include(p => p.CurrentPrice)
                        .FirstOrDefault(p => p.Id == detail.ProductId);

                    if (product == null)
                        throw new Exception($"Product {detail.ProductId} not found");

                    if (detail.CylinderUnitPrice <= 0)
                    {
                        detail.CylinderUnitPrice = product.CurrentPrice?.CylinderBuyingPrice ?? 0;
                    }

                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock == null)
                    {
                        stock = new ProductStock
                        {
                            ProductId = detail.ProductId,
                            LoadedQty = 0,
                            EmptyQty = 0
                        };
                        _dbContext.ProductStocks.Add(stock);
                    }

                    if (exchange.ExchangeType == "Give")
                    {
                        stock.EmptyQty -= detail.Quantity;
                    }
                    else // Receive
                    {
                        stock.EmptyQty += detail.Quantity;
                    }
                }

                // Step 3: Update header
                existingExchange.SupplierId = exchange.SupplierId;
                existingExchange.ExchangeType = exchange.ExchangeType;
                existingExchange.PaymentMethodId = exchange.PaymentMethodId;
                existingExchange.Date = exchange.Date;
                existingExchange.Comments = exchange.Comments;
                existingExchange.TotalPay = exchange.TotalPay;
                existingExchange.TotalAmount = exchange.ExchangeDetails
                    .Sum(d => d.Quantity * d.CylinderUnitPrice);
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
                // Step 5: Payment history update
                // ============================================
                var existingPayment = existingExchange.ExchangePaymentHistories
                    .FirstOrDefault(p => !p.IsDeleted);

                if (exchange.TotalPay > 0)
                {
                    if (existingPayment != null)
                    {
                        existingPayment.Amount = exchange.TotalPay;
                        existingPayment.Direction = exchange.ExchangeType;
                        existingPayment.PaymentMethodId = exchange.PaymentMethodId;
                        existingPayment.Date = exchange.Date;
                        existingPayment.Comments = exchange.Comments;
                    }
                    else
                    {
                        var newPayment = new ExchangePaymentHistory
                        {
                            CylinderExchangeId = exchange.Id,
                            Amount = exchange.TotalPay,
                            Direction = exchange.ExchangeType,
                            PaymentMethodId = exchange.PaymentMethodId,
                            Date = exchange.Date,
                            Comments = exchange.Comments,
                            IsDeleted = false
                        };
                        _dbContext.ExchangePaymentHistories.Add(newPayment);
                    }
                }
                else if (existingPayment != null)
                {
                    // TotalPay 0 hole payment history soft delete
                    existingPayment.IsDeleted = true;
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
                        {
                            // Age Give → EmptyQty komiyeche
                            // Reverse → EmptyQty BARABO
                            stock.EmptyQty += detail.Quantity;
                        }
                        else // Receive
                        {
                            // Age Receive → EmptyQty bariyeche
                            // Reverse → EmptyQty KOMABO
                            stock.EmptyQty -= detail.Quantity;
                        }
                    }
                }

                // Soft delete exchange
                exchange.IsDeleted = true;

                // Soft delete payments
                foreach (var payment in exchange.ExchangePaymentHistories)
                {
                    payment.IsDeleted = true;
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
                    .Include(e => e.Supplier)
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
                    .Include(e => e.Supplier)
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
        public CylinderExchange GetCylinderExchangeBySupplierId(int supplierId)
        {
            try
            {
                return _dbContext.CylinderExchanges
                    .Include(e => e.Supplier)
                    .Include(e => e.PaymentMethod)
                    .Include(e => e.ExchangeDetails)
                        .ThenInclude(d => d.Product)
                    .Include(e => e.ExchangePaymentHistories)
                    .Where(e => e.SupplierId == supplierId && !e.IsDeleted)
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
                            join s in _dbContext.Suppliers on ce.SupplierId equals s.Id into supplierJoin
                            from s in supplierJoin.DefaultIfEmpty()
                            where ce.Id == exchangeId && !ce.IsDeleted
                            select new CylinderExchangeDetailsDTO
                            {
                                CylinderExchangeId = ce.Id,
                                SupplierName = s != null ? s.Name : "",
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
    }
}