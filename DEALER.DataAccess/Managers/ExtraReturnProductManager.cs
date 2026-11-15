namespace DEALER.DataAccess
{
        public class ExtraReturnProductManager : BaseDataManager, IExtraReturnProduct
        {
            public ExtraReturnProductManager(DEALERContext model) : base(model)
            {
            }

        public bool CreateExtraReturnProduct(ExtraReturnProduct extraReturnProduct)
        {
            if (extraReturnProduct == null ||
                extraReturnProduct.ExtraReturnProductDetails == null ||
                !extraReturnProduct.ExtraReturnProductDetails.Any())
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // Order-e je product gulo chilo segulo ber kore niye ashi
                var orderProductIds = _dbContext.OrderDetails
                    .Where(od => od.OrderId == extraReturnProduct.OrderId)
                    .Select(od => od.ProductId)
                    .Distinct()
                    .ToHashSet();

                // Get product empty cylinder prices
                foreach (var detail in extraReturnProduct.ExtraReturnProductDetails)
                {
                    var product = _dbContext.Products
                        .Include(p => p.CurrentPrice)
                        .FirstOrDefault(p => p.Id == detail.ProductId);

                    if (product == null)
                        throw new Exception($"Product {detail.ProductId} not found");

                    // ✅ Order-e thakle SellingPrice, na thakle BuyingPrice
                    if (orderProductIds.Contains(detail.ProductId))
                    {
                        detail.EmptyCylinderPrice = product.CurrentPrice?.CylinderSellingPrice ?? 0;
                    }
                    else
                    {
                        detail.EmptyCylinderPrice = product.CurrentPrice?.CylinderBuyingPrice ?? 0;
                    }

                    detail.Price = detail.Quantity * detail.EmptyCylinderPrice;

                    // Update Stock - INCREASE EmptyQty for this product/company
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock == null)
                    {
                        stock = new ProductStock
                        {
                            ProductId = detail.ProductId,
                            LoadedQty = 0,
                            EmptyQty = detail.Quantity
                        };
                        _dbContext.ProductStocks.Add(stock);
                    }
                    else
                    {
                        stock.EmptyQty += detail.Quantity;
                    }
                }

                // Calculate total
                extraReturnProduct.TotalAmount = extraReturnProduct.ExtraReturnProductDetails
                    .Sum(d => d.Price);

                // Save ExtraReturnProduct
                _dbContext.ExtraReturnProducts.Add(extraReturnProduct);
                _dbContext.SaveChanges();

                // ... বাকি payment history কোড অপরিবর্তিত ...
                // Update Order with extra return info
                var order = _dbContext.Orders
                    .FirstOrDefault(o => o.Id == extraReturnProduct.OrderId);

                if (order != null)
                {
                    // You can add a field to track extra returns if needed
                }

                // Add to Customer Payment History (if employee exists)
                if (extraReturnProduct.EmployeeId > 0)
                {
                    var lastPayment = _dbContext.CustomerPaymentHistories
                        .Where(p => p.EmployeeId == extraReturnProduct.EmployeeId && !p.IsDeleted)
                        .OrderByDescending(p => p.Id)
                        .FirstOrDefault();

                    double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0;
                    double totalDueAfter = totalDueBefore - extraReturnProduct.TotalAmount;

                    var payment = new CustomerPaymentHistory
                    {
                        EmployeeId = extraReturnProduct.EmployeeId,
                        OrderId = extraReturnProduct.OrderId,
                        ExtraReturnProductId = extraReturnProduct.Id,
                        PaymentDate = extraReturnProduct.Date,
                        PaymentMethodId = 1, // Empty Cylinder Return
                        TransactionID = string.Empty,
                        Number = string.Empty,
                        TotalAmountThisOrder = extraReturnProduct.TotalAmount,
                        AmountPaid = extraReturnProduct.TotalAmount,
                        TotalDueBeforePayment = totalDueBefore,
                        TotalDueAfterPayment = totalDueAfter
                    };

                    _dbContext.CustomerPaymentHistories.Add(payment);
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateExtraReturnProduct(ExtraReturnProduct extraReturnProduct)
        {
            if (extraReturnProduct == null ||
                extraReturnProduct.ExtraReturnProductDetails == null ||
                !extraReturnProduct.ExtraReturnProductDetails.Any())
            {
                return false;
            }

            // Page theke asha tracked entity/temporary Id-gulo bad dewar jonno
            _dbContext.ChangeTracker.Clear();

            // Clear-er por-o page-er data thake, tai age theke plain list-e niye rakhi
            var incomingDetails = extraReturnProduct.ExtraReturnProductDetails
                .Select(d => new { d.ProductId, d.Quantity })
                .ToList();

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // DB theke fresh load (ekhon kono stale tracked object nai)
                var existingExtraReturn = _dbContext.ExtraReturnProducts
                    .Include(e => e.ExtraReturnProductDetails)
                    .FirstOrDefault(e => e.Id == extraReturnProduct.Id);

                if (existingExtraReturn == null)
                    return false;

                // Step 1: Purano stock change reverse kori
                foreach (var oldDetail in existingExtraReturn.ExtraReturnProductDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == oldDetail.ProductId);

                    if (stock != null)
                    {
                        stock.EmptyQty -= oldDetail.Quantity;
                    }
                }

                // Purano details delete (egulo DB theke load, tai Id permanent)
                var oldDetails = existingExtraReturn.ExtraReturnProductDetails.ToList();
                _dbContext.ExtraReturnProductDetails.RemoveRange(oldDetails);
                _dbContext.SaveChanges();

                // Order-e je product gulo chilo
                var orderProductIds = _dbContext.OrderDetails
                    .Where(od => od.OrderId == extraReturnProduct.OrderId)
                    .Select(od => od.ProductId)
                    .Distinct()
                    .ToHashSet();

                // Step 2: Notun detail gulo fresh object hishebe toiri
                var newDetails = new List<ExtraReturnProductDetails>();

                foreach (var detail in incomingDetails)
                {
                    var product = _dbContext.Products
                        .Include(p => p.CurrentPrice)
                        .FirstOrDefault(p => p.Id == detail.ProductId);

                    if (product == null)
                        throw new Exception($"Product {detail.ProductId} not found");

                    // Order-e thakle SellingPrice, na thakle BuyingPrice
                    double emptyPrice = orderProductIds.Contains(detail.ProductId)
                        ? (product.CurrentPrice?.CylinderSellingPrice ?? 0)
                        : (product.CurrentPrice?.CylinderBuyingPrice ?? 0);

                    newDetails.Add(new ExtraReturnProductDetails
                    {
                        ExtraReturnProductId = existingExtraReturn.Id,
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        EmptyCylinderPrice = emptyPrice,
                        Price = detail.Quantity * emptyPrice
                    });

                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock != null)
                    {
                        stock.EmptyQty += detail.Quantity;
                    }
                }

                // Header update
                existingExtraReturn.Date = extraReturnProduct.Date;
                existingExtraReturn.Remarks = extraReturnProduct.Remarks;
                existingExtraReturn.TotalAmount = newDetails.Sum(d => d.Price);

                // Notun details add
                _dbContext.ExtraReturnProductDetails.AddRange(newDetails);
                _dbContext.SaveChanges();

                // Payment history update
                if (extraReturnProduct.EmployeeId > 0)
                {
                    var payment = _dbContext.CustomerPaymentHistories
                        .FirstOrDefault(p => p.ExtraReturnProductId == existingExtraReturn.Id && !p.IsDeleted);

                    var lastPayment = _dbContext.CustomerPaymentHistories
                        .Where(p => p.EmployeeId == extraReturnProduct.EmployeeId && !p.IsDeleted
                                    && (payment == null || p.Id != payment.Id))
                        .OrderByDescending(p => p.Id)
                        .FirstOrDefault();

                    double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0;
                    double totalDueAfter = totalDueBefore - existingExtraReturn.TotalAmount;

                    if (payment != null)
                    {
                        payment.AmountPaid = existingExtraReturn.TotalAmount;
                        payment.TotalDueBeforePayment = totalDueBefore;
                        payment.TotalDueAfterPayment = totalDueAfter;
                        payment.PaymentDate = existingExtraReturn.Date;
                    }
                    else
                    {
                        _dbContext.CustomerPaymentHistories.Add(new CustomerPaymentHistory
                        {
                            EmployeeId = extraReturnProduct.EmployeeId,
                            OrderId = extraReturnProduct.OrderId,
                            ExtraReturnProductId = existingExtraReturn.Id,
                            PaymentDate = existingExtraReturn.Date,
                            PaymentMethodId = 1, // Empty Cylinder Return
                            TransactionID = string.Empty,
                            Number = string.Empty,
                            TotalAmountThisOrder = existingExtraReturn.TotalAmount,
                            AmountPaid = existingExtraReturn.TotalAmount,
                            TotalDueBeforePayment = totalDueBefore,
                            TotalDueAfterPayment = totalDueAfter
                        });
                    }
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        public bool DeleteExtraReturnProduct(int extraReturnProductId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var extraReturn = _dbContext.ExtraReturnProducts
                    .Include(e => e.ExtraReturnProductDetails)
                    .FirstOrDefault(e => e.Id == extraReturnProductId);

                if (extraReturn == null)
                    return false;

                if (extraReturn.IsDeleted)
                    return true;

                // Reverse stock
                foreach (var detail in extraReturn.ExtraReturnProductDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock != null)
                    {
                        stock.EmptyQty -= detail.Quantity;
                    }
                }

                // Soft delete
                extraReturn.IsDeleted = true;

                // Soft delete payment
                var payments = _dbContext.CustomerPaymentHistories
                    .Where(p => p.ExtraReturnProductId == extraReturnProductId && !p.IsDeleted)
                    .ToList();

                foreach (var payment in payments)
                {
                    payment.IsDeleted = true;
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<ExtraReturnProduct> GetExtraReturnProductByOrderId(int orderId)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return null;

                // Check if extra return already exists
                var existingExtraReturn = await _dbContext.ExtraReturnProducts
                    .Include(e => e.ExtraReturnProductDetails)
                    .FirstOrDefaultAsync(e => e.OrderId == orderId && !e.IsDeleted);

                if (existingExtraReturn != null)
                {
                    return existingExtraReturn;
                }

                // ✅ এখানে বসান — Create new empty extra return with products from order
                var extraReturn = new ExtraReturnProduct
                {
                    OrderId = orderId,
                    EmployeeId = order.EmployeeId ?? 0,
                    Date = DateTime.UtcNow,
                    TotalAmount = 0,
                    ExtraReturnProductDetails = order.OrderDetails
                        .GroupBy(od => od.ProductId)   // same product ekadhik bar hole merge
                        .Select(g =>
                        {
                            var product = _dbContext.Products
                                .Include(p => p.CurrentPrice)
                                .FirstOrDefault(p => p.Id == g.Key);

                            var sellingPrice = product?.CurrentPrice?.CylinderSellingPrice ?? 0;
                            var qty = g.Sum(x => x.Quantity);

                            return new ExtraReturnProductDetails
                            {
                                ProductId = g.Key,
                                Quantity = qty,
                                EmptyCylinderPrice = sellingPrice,
                                Price = qty * sellingPrice
                            };
                        }).ToList()
                };

                return extraReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ExtraReturnProduct> GetExtraReturnProductById(int id)
        {
            try
            {
                return await _dbContext.ExtraReturnProducts
                    .Include(e => e.ExtraReturnProductDetails)
                    .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<ExtraReturnProductDetailsDto>> GetExtraReturnProductDetailsByOrderIdAsync(int orderId)
        {
            var query = from erp in _dbContext.ExtraReturnProducts
                        join erpd in _dbContext.ExtraReturnProductDetails on erp.Id equals erpd.ExtraReturnProductId
                        join p in _dbContext.Products on erpd.ProductId equals p.Id
                        join s in _dbContext.Suppliers on p.SupplierId equals s.Id
                        where erp.OrderId == orderId && !erp.IsDeleted
                        select new ExtraReturnProductDetailsDto
                        {
                            ExtraReturnProductId = erp.Id,
                            ProductId = erpd.ProductId,
                            ProductName = p.DisplayNameSize,
                            SupplierId = s.Id,
                            SupplierName = s.Name,
                            Quantity = erpd.Quantity,
                            EmptyCylinderPrice = erpd.EmptyCylinderPrice,
                            TotalAmount = erpd.Price,
                            ReturnDate = erp.Date,
                            Remarks = erp.Remarks
                        };

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ExtraReturnProductGroupDto>> GetExtraReturnProductGroupedByCompanyAsync(int orderId)
        {
            var details = await GetExtraReturnProductDetailsByOrderIdAsync(orderId);

            var grouped = details
                .GroupBy(d => new { d.SupplierId, d.SupplierName })
                .Select(g => new ExtraReturnProductGroupDto
                {
                    SupplierName = g.Key.SupplierName,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalAmount = g.Sum(x => x.TotalAmount),
                    Details = g.ToList()
                })
                .OrderBy(g => g.SupplierName)
                .ToList();

            return grouped;
        }

    }
}