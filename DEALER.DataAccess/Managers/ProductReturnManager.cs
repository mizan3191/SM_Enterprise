namespace DEALER.DataAccess
{
    public class ProductReturnManager : BaseDataManager, IProductReturn
    {
        public ProductReturnManager(DEALERContext model) : base(model)
        {
        }

        public bool CreateCustomerProductReturn(CustomerProductReturn customerProductReturn)
        {
            if (customerProductReturn == null ||
                customerProductReturn.CustomerProductReturnDetails == null ||
                !customerProductReturn.CustomerProductReturnDetails.Any())
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // Get the original order to validate return quantities
                var order = _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.Id == customerProductReturn.OrderId);

                if (order == null)
                    throw new Exception("Original order not found");

                // Validate return quantities against original order
                foreach (var detail in customerProductReturn.CustomerProductReturnDetails)
                {
                    var orderDetail = order.OrderDetails
                        .FirstOrDefault(od => od.ProductId == detail.ProductId);

                    if (orderDetail == null)
                        throw new Exception($"Product {detail.ProductId} not found in original order");

                    // Check if already returned
                    var existingReturn = _dbContext.CustomerProductReturnDetails
                        .Where(r => r.CustomerProductReturn.OrderId == customerProductReturn.OrderId
                                    && r.ProductId == detail.ProductId)
                        .Sum(r => r.ReturnQuantity);

                    var availableToReturn = orderDetail.Quantity - existingReturn;

                    if (detail.ReturnQuantity > availableToReturn)
                        throw new Exception($"Cannot return more than {availableToReturn} for product {detail.ProductId}");

                    // Set prices from original order
                    detail.CylinderUnitPrice = orderDetail.CylinderUnitPrice;
                    detail.GasUnitPrice = orderDetail.GasUnitPrice;
                    detail.OrderQuantity = orderDetail.Quantity;

                    // Calculate return prices
                    detail.CylinderReturnPrice = detail.CylinderUnitPrice * detail.ReturnQuantity;
                    detail.GasReturnPrice = detail.GasUnitPrice * detail.ReturnQuantity;

                    var subtotal = detail.CylinderReturnPrice + detail.GasReturnPrice;
                    detail.ReturnPrice = subtotal - (subtotal * (detail.Discount / 100));
                }

                // Calculate total
                customerProductReturn.TotalAmount = customerProductReturn.CustomerProductReturnDetails
                    .Sum(d => d.CylinderReturnPrice + d.GasReturnPrice);
                customerProductReturn.TotalReturnPrice = customerProductReturn.CustomerProductReturnDetails
                    .Sum(d => d.ReturnPrice);

                // Save the return
                _dbContext.CustomerProductReturns.Add(customerProductReturn);
                _dbContext.SaveChanges();

                // Update stock - INCREASE stock (return means products coming back)
                foreach (var detail in customerProductReturn.CustomerProductReturnDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock not found for product {detail.ProductId}");

                    // Increase LoadedQty (returned loaded cylinders)
                    stock.LoadedQty += detail.ReturnQuantity;

                    // Update OrderDetail ReturnQuantity
                    var orderDetail = _dbContext.OrderDetails
                        .FirstOrDefault(od => od.OrderId == customerProductReturn.OrderId
                                              && od.ProductId == detail.ProductId);

                    if (orderDetail != null)
                    {
                        orderDetail.ReturnQuantity = (orderDetail.ReturnQuantity ?? 0) + detail.ReturnQuantity;
                    }
                }

                _dbContext.SaveChanges();

                // Update customer payment (if customer is associated)
                if (customerProductReturn.CustomerId.HasValue && customerProductReturn.CustomerId.Value > 0)
                {
                    UpdateCustomerPaymentHistory(customerProductReturn);
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateCustomerProductReturn(CustomerProductReturn customerProductReturn)
        {
            if (customerProductReturn == null ||
                customerProductReturn.CustomerProductReturnDetails == null ||
                !customerProductReturn.CustomerProductReturnDetails.Any())
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingReturn = _dbContext.CustomerProductReturns
                    .Include(r => r.CustomerProductReturnDetails)
                    .FirstOrDefault(r => r.Id == customerProductReturn.Id);

                if (existingReturn == null)
                    return false;

                // Step 1: Reverse old stock changes
                foreach (var oldDetail in existingReturn.CustomerProductReturnDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == oldDetail.ProductId);

                    if (stock != null)
                    {
                        stock.LoadedQty -= oldDetail.ReturnQuantity;
                    }

                    // Reverse OrderDetail ReturnQuantity
                    var orderDetail = _dbContext.OrderDetails
                        .FirstOrDefault(od => od.OrderId == existingReturn.OrderId
                                              && od.ProductId == oldDetail.ProductId);

                    if (orderDetail != null)
                    {
                        orderDetail.ReturnQuantity = (orderDetail.ReturnQuantity ?? 0) - oldDetail.ReturnQuantity;
                    }
                }

                _dbContext.SaveChanges();

                // Step 2: Remove old details and add new ones
                _dbContext.CustomerProductReturnDetails.RemoveRange(existingReturn.CustomerProductReturnDetails);
                _dbContext.SaveChanges();

                // Step 3: Apply new stock changes
                foreach (var detail in customerProductReturn.CustomerProductReturnDetails)
                {
                    detail.CustomerProductReturnId = customerProductReturn.Id;

                    // Validate against original order
                    var orderDetail = _dbContext.OrderDetails
                        .FirstOrDefault(od => od.OrderId == customerProductReturn.OrderId
                                              && od.ProductId == detail.ProductId);

                    if (orderDetail == null)
                        throw new Exception($"Product {detail.ProductId} not found in original order");

                    // Check total returned including other returns
                    var totalReturned = _dbContext.CustomerProductReturnDetails
                        .Where(r => r.CustomerProductReturn.OrderId == customerProductReturn.OrderId
                                    && r.ProductId == detail.ProductId
                                    && r.CustomerProductReturnId != customerProductReturn.Id)
                        .Sum(r => r.ReturnQuantity);

                    var availableToReturn = orderDetail.Quantity - totalReturned;

                    if (detail.ReturnQuantity > availableToReturn)
                        throw new Exception($"Cannot return more than {availableToReturn} for product {detail.ProductId}");

                    // Set prices
                    detail.CylinderUnitPrice = orderDetail.CylinderUnitPrice;
                    detail.GasUnitPrice = orderDetail.GasUnitPrice;
                    detail.OrderQuantity = orderDetail.Quantity;

                    // Calculate return prices
                    detail.CylinderReturnPrice = detail.CylinderUnitPrice * detail.ReturnQuantity;
                    detail.GasReturnPrice = detail.GasUnitPrice * detail.ReturnQuantity;

                    var subtotal = detail.CylinderReturnPrice + detail.GasReturnPrice;
                    detail.ReturnPrice = subtotal - (subtotal * (detail.Discount / 100));

                    // Update stock
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock != null)
                    {
                        stock.LoadedQty += detail.ReturnQuantity;
                    }

                    // Update OrderDetail ReturnQuantity
                    orderDetail.ReturnQuantity = (orderDetail.ReturnQuantity ?? 0) + detail.ReturnQuantity;
                }

                // Update return header
                existingReturn.Date = customerProductReturn.Date;
                existingReturn.Discount = customerProductReturn.Discount;
                existingReturn.Remarks = customerProductReturn.Remarks;
                existingReturn.TotalAmount = customerProductReturn.CustomerProductReturnDetails
                    .Sum(d => d.CylinderReturnPrice + d.GasReturnPrice);
                existingReturn.TotalReturnPrice = customerProductReturn.CustomerProductReturnDetails
                    .Sum(d => d.ReturnPrice);

                // Add new details
                foreach (var detail in customerProductReturn.CustomerProductReturnDetails)
                {
                    existingReturn.CustomerProductReturnDetails.Add(detail);
                }

                _dbContext.SaveChanges();

                // Update customer payment
                if (existingReturn.CustomerId.HasValue && existingReturn.CustomerId.Value > 0)
                {
                    UpdateCustomerPaymentHistory(existingReturn);
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }


        public bool DeleteCustomerProductReturn(int customerProductReturnId)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var customerProductReturn = _dbContext.CustomerProductReturns
                    .Include(r => r.CustomerProductReturnDetails)
                    .FirstOrDefault(r => r.Id == customerProductReturnId);

                if (customerProductReturn == null)
                    return false;

                if (customerProductReturn.IsDeleted)
                    return true;

                // Reverse stock changes
                foreach (var detail in customerProductReturn.CustomerProductReturnDetails)
                {
                    var stock = _dbContext.ProductStocks
                        .FirstOrDefault(s => s.ProductId == detail.ProductId);

                    if (stock != null)
                    {
                        stock.LoadedQty -= detail.ReturnQuantity;
                    }

                    // Reverse OrderDetail ReturnQuantity
                    var orderDetail = _dbContext.OrderDetails
                        .FirstOrDefault(od => od.OrderId == customerProductReturn.OrderId
                                              && od.ProductId == detail.ProductId);

                    if (orderDetail != null)
                    {
                        orderDetail.ReturnQuantity = (orderDetail.ReturnQuantity ?? 0) - detail.ReturnQuantity;
                    }
                }

                // Soft delete
                customerProductReturn.IsDeleted = true;

                // Soft delete payment history
                var payments = _dbContext.CustomerPaymentHistories
                    .Where(p => p.CustomerProductReturnId == customerProductReturnId && !p.IsDeleted)
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

        private void UpdateCustomerPaymentHistory(CustomerProductReturn customerProductReturn)
        {
            if (!customerProductReturn.CustomerId.HasValue)
                return;

            var lastPayment = _dbContext.CustomerPaymentHistories
                .Where(p => p.CustomerId == customerProductReturn.CustomerId.Value && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0;
            double totalDueAfter = totalDueBefore - customerProductReturn.TotalReturnPrice;

            // Check if payment already exists for this return
            var existingPayment = _dbContext.CustomerPaymentHistories
                .FirstOrDefault(p => p.CustomerProductReturnId == customerProductReturn.Id && !p.IsDeleted);

            if (existingPayment != null)
            {
                existingPayment.AmountPaid = customerProductReturn.TotalReturnPrice;
                existingPayment.TotalDueBeforePayment = totalDueBefore;
                existingPayment.TotalDueAfterPayment = totalDueAfter;
                existingPayment.PaymentDate = customerProductReturn.Date;
            }
            else
            {
                var payment = new CustomerPaymentHistory
                {
                    CustomerId = customerProductReturn.CustomerId.Value,
                    OrderId = customerProductReturn.OrderId,
                    CustomerProductReturnId = customerProductReturn.Id,
                    PaymentDate = customerProductReturn.Date,
                    PaymentMethodId = 1, //Product Return
                    TransactionID = string.Empty,
                    Number = string.Empty,
                    TotalAmountThisOrder = customerProductReturn.TotalReturnPrice,
                    AmountPaid = customerProductReturn.TotalReturnPrice,
                    TotalDueBeforePayment = totalDueBefore,
                    TotalDueAfterPayment = totalDueAfter
                };

                _dbContext.CustomerPaymentHistories.Add(payment);
            }
        }

        public async Task<CustomerProductReturn> GetCustomerProductReturnByOrderId(int orderId)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return null;

                // Check for existing return
                var existingReturn = await _dbContext.CustomerProductReturns
                    .Include(r => r.CustomerProductReturnDetails)
                    .FirstOrDefaultAsync(r => r.OrderId == orderId && !r.IsDeleted);

                if (existingReturn != null)
                {
                    // Update with latest order details
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        var existingDetail = existingReturn.CustomerProductReturnDetails
                            .FirstOrDefault(d => d.ProductId == orderDetail.ProductId);

                        if (existingDetail != null)
                        {
                            existingDetail.OrderQuantity = orderDetail.Quantity;
                            existingDetail.CylinderUnitPrice = orderDetail.CylinderUnitPrice;
                            existingDetail.GasUnitPrice = orderDetail.GasUnitPrice;
                        }
                        else
                        {
                            var newDetail = new CustomerProductReturnDetails
                            {
                                CustomerProductReturnId = existingReturn.Id,
                                ProductId = orderDetail.ProductId,
                                OrderQuantity = orderDetail.Quantity,
                                ReturnQuantity = 0,
                                CylinderUnitPrice = orderDetail.CylinderUnitPrice,
                                GasUnitPrice = orderDetail.GasUnitPrice,
                                Discount = 0,
                                CylinderReturnPrice = 0,
                                GasReturnPrice = 0,
                                ReturnPrice = 0
                            };
                            existingReturn.CustomerProductReturnDetails.Add(newDetail);
                        }
                    }

                    // Remove products no longer in order
                    var productIds = order.OrderDetails.Select(od => od.ProductId).ToList();
                    var toRemove = existingReturn.CustomerProductReturnDetails
                        .Where(d => !productIds.Contains(d.ProductId))
                        .ToList();

                    foreach (var item in toRemove)
                    {
                        existingReturn.CustomerProductReturnDetails.Remove(item);
                    }

                    await _dbContext.SaveChangesAsync();
                    return existingReturn;
                }

                // Create new return with all products
                var newReturn = new CustomerProductReturn
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    EmployeeId = order.EmployeeId,
                    Date = DateTime.UtcNow,
                    TotalAmount = 0,
                    TotalReturnPrice = 0,
                    CustomerProductReturnDetails = order.OrderDetails.Select(od => new CustomerProductReturnDetails
                    {
                        ProductId = od.ProductId,
                        OrderQuantity = od.Quantity,
                        ReturnQuantity = 0,
                        CylinderUnitPrice = od.CylinderUnitPrice,
                        GasUnitPrice = od.GasUnitPrice,
                        Discount = 0,
                        CylinderReturnPrice = 0,
                        GasReturnPrice = 0,
                        ReturnPrice = 0
                    }).ToList()
                };

                return newReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CustomerProductReturn> GetCustomerProductReturnById(int id)
        {
            try
            {
                return await _dbContext.CustomerProductReturns
                    .Include(r => r.CustomerProductReturnDetails)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<CustomerProductReturnDTO>> GetAllCustomerReturnProducts()
        {
            try
            {
                return await _dbContext.CustomerProductReturns
                    .Include(r => r.Customer)
                    .Include(r => r.CustomerProductReturnDetails)
                        .ThenInclude(d => d.Product)
                    .Where(r => !r.IsDeleted)
                    .OrderByDescending(r => r.Id)
                    .Select(r => new CustomerProductReturnDTO
                    {
                        Id = r.Id,
                        OrderId = r.OrderId,
                        CustomerName = r.Customer != null ? r.Customer.Name : "N/A",
                        Products = string.Join(", ",
                            r.CustomerProductReturnDetails.Select(d =>
                                $"{d.Product.Name}({d.ReturnQuantity})")),
                        OrderDate = r.Date,
                        TotalPrice = r.TotalReturnPrice
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<CustomerProductReturnDTO>();
            }
        }

        public async Task<CustomerProductReturn> GetExistingCustomerProductReturnByOrderId(int orderId)
        {
            try
            {
                CustomerProductReturn customerProductReturn = await _dbContext.CustomerProductReturns
                    .Include(o => o.CustomerProductReturnDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (customerProductReturn == null)
                    return null;

                return customerProductReturn;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                return null;
            }
        }
    }
}