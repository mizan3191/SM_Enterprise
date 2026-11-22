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

                // Update customer payment (if employee is associated)
                if (customerProductReturn.EmployeeId.HasValue && customerProductReturn.EmployeeId.Value > 0)
                {
                    UpdateCustomerPaymentHistory(customerProductReturn);
                    //_dbContext.SaveChanges();
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

                // Posted list e same product ekadhik bar thakle ek row e merge kori (qty sum)
                var incomingDetails = customerProductReturn.CustomerProductReturnDetails
                    .GroupBy(d => d.ProductId)
                    .Select(g => new CustomerProductReturnDetails
                    {
                        ProductId = g.Key,
                        ReturnQuantity = g.Sum(x => x.ReturnQuantity),
                        Discount = g.First().Discount
                    })
                    .ToList();

                var existingDetails = existingReturn.CustomerProductReturnDetails.ToList();
                var orderDetails = new Dictionary<int, OrderDetail>();

                // Step 1: kichu change korar age sob validate
                foreach (var detail in incomingDetails)
                {
                    if (!orderDetails.TryGetValue(detail.ProductId, out var orderDetail))
                    {
                        orderDetail = _dbContext.OrderDetails
                            .FirstOrDefault(od => od.OrderId == existingReturn.OrderId
                                                  && od.ProductId == detail.ProductId);

                        if (orderDetail == null)
                            throw new Exception($"Product {detail.ProductId} not found in original order");

                        orderDetails[detail.ProductId] = orderDetail;
                    }

                    var returnedByOtherReturns = _dbContext.CustomerProductReturnDetails
                        .Where(r => r.CustomerProductReturn.OrderId == existingReturn.OrderId
                                    && r.ProductId == detail.ProductId
                                    && r.CustomerProductReturnId != existingReturn.Id)
                        .Sum(r => r.ReturnQuantity);

                    var availableToReturn = orderDetail.Quantity - returnedByOtherReturns;

                    if (detail.ReturnQuantity > availableToReturn)
                        throw new Exception($"Cannot return more than {availableToReturn} for product {detail.ProductId}");
                }

                // Step 2: je product gulo ekhon return list e nei, segulo remove + delta reverse
                var incomingProductIds = incomingDetails.Select(d => d.ProductId).ToHashSet();

                foreach (var oldDetail in existingDetails
                             .Where(d => !incomingProductIds.Contains(d.ProductId))
                             .ToList())
                {
                    ApplyReturnDelta(existingReturn.OrderId, oldDetail.ProductId, -oldDetail.ReturnQuantity);
                    _dbContext.CustomerProductReturnDetails.Remove(oldDetail);
                }

                // Step 3: existing row update / notun row add — sudhu delta apply
                var finalDetails = new List<CustomerProductReturnDetails>();

                foreach (var detail in incomingDetails)
                {
                    var orderDetail = orderDetails[detail.ProductId];
                    var existingDetail = existingDetails
                        .FirstOrDefault(d => d.ProductId == detail.ProductId);

                    var oldQty = existingDetail?.ReturnQuantity ?? 0;
                    var delta = detail.ReturnQuantity - oldQty;

                    CustomerProductReturnDetails target;

                    if (existingDetail != null)
                    {
                        target = existingDetail;
                        target.ReturnQuantity = detail.ReturnQuantity;
                        target.Discount = detail.Discount;
                    }
                    else
                    {
                        detail.Id = 0;
                        detail.CustomerProductReturn = null; // tracked header er sathe conflict e jeno na jay
                        detail.CustomerProductReturnId = existingReturn.Id;
                        existingReturn.CustomerProductReturnDetails.Add(detail);
                        target = detail;
                    }

                    target.CylinderUnitPrice = orderDetail.CylinderUnitPrice;
                    target.GasUnitPrice = orderDetail.GasUnitPrice;
                    target.OrderQuantity = orderDetail.Quantity;

                    target.CylinderReturnPrice = target.CylinderUnitPrice * target.ReturnQuantity;
                    target.GasReturnPrice = target.GasUnitPrice * target.ReturnQuantity;

                    var subtotal = target.CylinderReturnPrice + target.GasReturnPrice;
                    target.ReturnPrice = subtotal - (subtotal * (target.Discount / 100));

                    if (delta != 0)
                        ApplyReturnDelta(existingReturn.OrderId, detail.ProductId, delta);

                    finalDetails.Add(target);
                }

                // Step 4: header update
                existingReturn.Date = customerProductReturn.Date;
                existingReturn.Discount = customerProductReturn.Discount;
                existingReturn.Remarks = customerProductReturn.Remarks;
                existingReturn.TotalAmount = finalDetails
                    .Sum(d => d.CylinderReturnPrice + d.GasReturnPrice);
                existingReturn.TotalReturnPrice = finalDetails.Sum(d => d.ReturnPrice);

                _dbContext.SaveChanges();

                // Step 5: customer payment (age ei SaveChanges ta miss chilo)
                if (existingReturn.EmployeeId.HasValue && existingReturn.EmployeeId.Value > 0)
                {
                    UpdateCustomerPaymentHistory(existingReturn);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private void ApplyReturnDelta(int orderId, int productId, int qtyDelta)
        {
            var stock = _dbContext.ProductStocks
                .FirstOrDefault(s => s.ProductId == productId);

            if (stock == null)
                throw new Exception($"Stock not found for product {productId}");

            stock.LoadedQty += qtyDelta;

            var orderDetail = _dbContext.OrderDetails
                .FirstOrDefault(od => od.OrderId == orderId && od.ProductId == productId);

            if (orderDetail != null)
                orderDetail.ReturnQuantity = (orderDetail.ReturnQuantity ?? 0) + qtyDelta;
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
            if (!customerProductReturn.EmployeeId.HasValue)
                return;

            var employeeId = customerProductReturn.EmployeeId.Value;

            var existingPayment = _dbContext.CustomerPaymentHistories
                .FirstOrDefault(p => p.CustomerProductReturnId == customerProductReturn.Id && !p.IsDeleted);

            // ---------- CREATE path ----------
            if (existingPayment == null)
            {
                var lastPayment = _dbContext.CustomerPaymentHistories
                    .Where(p => p.EmployeeId == employeeId && !p.IsDeleted)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault();

                double totalDueBefore2 = lastPayment?.TotalDueAfterPayment ?? 0;
                double totalDueAfter2 = totalDueBefore2 - customerProductReturn.TotalReturnPrice;

                _dbContext.CustomerPaymentHistories.Add(new CustomerPaymentHistory
                {
                    EmployeeId = employeeId,
                    OrderId = customerProductReturn.OrderId,
                    CustomerProductReturnId = customerProductReturn.Id,
                    PaymentDate = customerProductReturn.Date,
                    PaymentMethodId = 5, // Product Return
                    TransactionID = string.Empty,
                    Number = string.Empty,
                    TotalAmountThisOrder = customerProductReturn.TotalReturnPrice,
                    AmountPaid = customerProductReturn.TotalReturnPrice,
                    TotalDueBeforePayment = totalDueBefore2,
                    TotalDueAfterPayment = totalDueAfter2
                });

                _dbContext.SaveChanges();
                return;
            }

            // ---------- UPDATE path ----------
            // Ei return er "due before" oi return er payment record er age je payment ta ache,
            // tar theke nite hobe. Last payment theke nile record ta nijei dhora pore effect double hoy.
            var previousPayment = _dbContext.CustomerPaymentHistories
                .Where(p => p.EmployeeId == employeeId
                            && !p.IsDeleted
                            && p.Id < existingPayment.Id)
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            double totalDueBefore = previousPayment?.TotalDueAfterPayment ?? 0;
            double totalDueAfter = totalDueBefore - customerProductReturn.TotalReturnPrice;

            // Ei record er effect jodi bodlay, tahole porer sob payment er running balance o
            // ei difference diye shift hote hobe
            double oldAmount = existingPayment.AmountPaid;
            double dueCorrection = oldAmount - customerProductReturn.TotalReturnPrice;

            existingPayment.AmountPaid = customerProductReturn.TotalReturnPrice;
            existingPayment.TotalAmountThisOrder = customerProductReturn.TotalReturnPrice;
            existingPayment.TotalDueBeforePayment = totalDueBefore;
            existingPayment.TotalDueAfterPayment = totalDueAfter;
            existingPayment.PaymentDate = customerProductReturn.Date;

            if (dueCorrection != 0)
            {
                var laterPayments = _dbContext.CustomerPaymentHistories
                    .Where(p => p.EmployeeId == employeeId
                                && !p.IsDeleted
                                && p.Id > existingPayment.Id)
                    .OrderBy(p => p.Id)
                    .ToList();

                foreach (var payment in laterPayments)
                {
                    payment.TotalDueBeforePayment += dueCorrection;
                    payment.TotalDueAfterPayment += dueCorrection;
                }
            }

            _dbContext.SaveChanges();
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
                    .Include(r => r.Employee)
                    .Include(r => r.CustomerProductReturnDetails)
                        .ThenInclude(d => d.Product)
                    .Where(r => !r.IsDeleted)
                    .OrderByDescending(r => r.Id)
                    .Select(r => new CustomerProductReturnDTO
                    {
                        Id = r.Id,
                        OrderId = r.OrderId,
                        EmployeeName = r.Employee != null ? r.Employee.Name : "N/A",
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