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
                // Get product empty cylinder prices
                foreach (var detail in extraReturnProduct.ExtraReturnProductDetails)
                {
                    var product = _dbContext.Products
                        .Include(p => p.CurrentPrice)
                        .FirstOrDefault(p => p.Id == detail.ProductId);

                    if (product == null)
                        throw new Exception($"Product {detail.ProductId} not found");

                    // Set empty cylinder price from product's buying price or a separate field
                    detail.EmptyCylinderPrice = product.CurrentPrice?.CylinderBuyingPrice ?? 0;
                    detail.Price = detail.Quantity * detail.EmptyCylinderPrice;

                    // Update Stock - INCREASE EmptyQty for this product/company
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock == null)
                    {
                        // Create stock if not exists
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

                // Update Order with extra return info
                var order = _dbContext.Orders
                    .FirstOrDefault(o => o.Id == extraReturnProduct.OrderId);

                if (order != null)
                {
                    // You can add a field to track extra returns if needed
                }

                // Add to Customer Payment History (if customer exists)
                if (extraReturnProduct.CustomerId.HasValue && extraReturnProduct.CustomerId.Value > 0)
                {
                    var lastPayment = _dbContext.CustomerPaymentHistories
                        .Where(p => p.CustomerId == extraReturnProduct.CustomerId.Value && !p.IsDeleted)
                        .OrderByDescending(p => p.Id)
                        .FirstOrDefault();

                    double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0;
                    double totalDueAfter = totalDueBefore - extraReturnProduct.TotalAmount;

                    var payment = new CustomerPaymentHistory
                    {
                        CustomerId = extraReturnProduct.CustomerId.Value,
                        OrderId = extraReturnProduct.OrderId,
                        ExtraReturnProductId = extraReturnProduct.Id,
                        PaymentDate = extraReturnProduct.Date,
                        PaymentMethodId = 1, // Return/Adjustment
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

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingExtraReturn = _dbContext.ExtraReturnProducts
                    .Include(e => e.ExtraReturnProductDetails)
                    .FirstOrDefault(e => e.Id == extraReturnProduct.Id);

                if (existingExtraReturn == null)
                    return false;

                // Step 1: Reverse old stock changes
                foreach (var oldDetail in existingExtraReturn.ExtraReturnProductDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == oldDetail.ProductId);

                    if (stock != null)
                    {
                        stock.EmptyQty -= oldDetail.Quantity;
                    }
                }

                // Remove old details
                _dbContext.ExtraReturnProductDetails.RemoveRange(existingExtraReturn.ExtraReturnProductDetails);
                _dbContext.SaveChanges();

                // Step 2: Apply new stock changes
                foreach (var detail in extraReturnProduct.ExtraReturnProductDetails)
                {
                    var product = _dbContext.Products
                        .Include(p => p.CurrentPrice)
                        .FirstOrDefault(p => p.Id == detail.ProductId);

                    if (product == null)
                        throw new Exception($"Product {detail.ProductId} not found");

                    detail.EmptyCylinderPrice = product.CurrentPrice?.CylinderBuyingPrice ?? 0;
                    detail.Price = detail.Quantity * detail.EmptyCylinderPrice;

                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock != null)
                    {
                        stock.EmptyQty += detail.Quantity;
                    }
                }

                // Update header
                existingExtraReturn.Date = extraReturnProduct.Date;
                existingExtraReturn.Remarks = extraReturnProduct.Remarks;
                existingExtraReturn.TotalAmount = extraReturnProduct.ExtraReturnProductDetails
                    .Sum(d => d.Price);

                // Add new details
                foreach (var detail in extraReturnProduct.ExtraReturnProductDetails)
                {
                    detail.ExtraReturnProductId = extraReturnProduct.Id;
                    existingExtraReturn.ExtraReturnProductDetails.Add(detail);
                }

                _dbContext.SaveChanges();

                // Update payment history
                if (extraReturnProduct.CustomerId.HasValue && extraReturnProduct.CustomerId.Value > 0)
                {
                    var payment = _dbContext.CustomerPaymentHistories
                        .FirstOrDefault(p => p.ExtraReturnProductId == extraReturnProduct.Id && !p.IsDeleted);

                    var lastPayment = _dbContext.CustomerPaymentHistories
                        .Where(p => p.CustomerId == extraReturnProduct.CustomerId.Value && !p.IsDeleted
                                    && (payment == null || p.Id != payment.Id))
                        .OrderByDescending(p => p.Id)
                        .FirstOrDefault();

                    double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0;
                    double totalDueAfter = totalDueBefore - extraReturnProduct.TotalAmount;

                    if (payment != null)
                    {
                        payment.AmountPaid = extraReturnProduct.TotalAmount;
                        payment.TotalDueBeforePayment = totalDueBefore;
                        payment.TotalDueAfterPayment = totalDueAfter;
                        payment.PaymentDate = extraReturnProduct.Date;
                    }
                    else
                    {
                        var newPayment = new CustomerPaymentHistory
                        {
                            CustomerId = extraReturnProduct.CustomerId.Value,
                            OrderId = extraReturnProduct.OrderId,
                            ExtraReturnProductId = extraReturnProduct.Id,
                            PaymentDate = extraReturnProduct.Date,
                            PaymentMethodId = 11,
                            TransactionID = string.Empty,
                            Number = string.Empty,
                            TotalAmountThisOrder = extraReturnProduct.TotalAmount,
                            AmountPaid = extraReturnProduct.TotalAmount,
                            TotalDueBeforePayment = totalDueBefore,
                            TotalDueAfterPayment = totalDueAfter
                        };
                        _dbContext.CustomerPaymentHistories.Add(newPayment);
                    }
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

                // Create new empty extra return with products from order
                var extraReturn = new ExtraReturnProduct
                {
                    OrderId = orderId,
                    CustomerId = order.CustomerId,
                    EmployeeId = order.EmployeeId ?? 0,
                    Date = DateTime.UtcNow,
                    TotalAmount = 0,
                    ExtraReturnProductDetails = order.OrderDetails.Select(od => new ExtraReturnProductDetails
                    {
                        ProductId = od.ProductId,
                        Quantity = 1,
                        EmptyCylinderPrice = od.CylinderUnitPrice, // Ekhane CylinderUnitPrice bosbe
                        Price = od.CylinderUnitPrice,
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

     

    }
}