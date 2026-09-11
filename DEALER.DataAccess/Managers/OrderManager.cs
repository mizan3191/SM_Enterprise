using DEALER.Domain;

namespace DEALER.DataAccess
{
    public class OrderManager : BaseDataManager, IOrder
    {
        public OrderManager(DEALERContext model) : base(model)
        {
        }

        public bool CreateOrder(Order order)
        {
            if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                _dbContext.Orders.Add(order);
                _dbContext.SaveChanges();

                foreach (var item in order.OrderDetails)
                {
                    item.OrderId = order.Id;

                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == item.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock info not found for product {item.ProductId}");

                    if (stock.LoadedQty < item.Quantity)
                        throw new Exception($"Insufficient loaded cylinder stock for product {item.ProductId}");

                    ApplyStockForSale(stock, item);
                }

                _dbContext.SaveChanges();

                if (order.TotalPay > 0)
                {
                    var lastBalance = _dbContext.TransactionHistories
                        .Where(x => !x.IsDeleted)
                        .OrderByDescending(x => x.Id)
                        .Select(x => x.CurrentBalance)
                        .FirstOrDefault() ?? 0;

                    var history = new TransactionHistory
                    {
                        BalanceIn = order.TotalPay,
                        BalanceOut = 0,
                        CurrentBalance = lastBalance + order.TotalPay,
                        Date = order.Date,
                        OrderId = order.Id,
                        Resone = "Sales Product"
                    };

                    _dbContext.TransactionHistories.Add(history);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateOrder(Order order)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // Comparison-এর জন্য পুরনো state (untracked snapshot)
                var existingOrderData = _dbContext.Orders
                   .Include(o => o.OrderDetails)
                   .AsNoTracking()
                   .FirstOrDefault(o => o.Id == order.Id);

                // Update করার জন্য tracked entity
                var existingOrder = _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.Id == order.Id);

                if (existingOrder == null)
                    return false;

                // --- Step 1: পুরনো order details এর stock effect আগে reverse করো ---
                var oldDetails = existingOrderData?.OrderDetails ?? new List<OrderDetail>();
                foreach (var oldDetail in oldDetails)
                {
                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == oldDetail.ProductId);
                    if (stock != null)
                    {
                        ReverseStockForSale(stock, oldDetail);
                    }
                }

                // Update order header
                existingOrder.Date = order.Date;
                existingOrder.TotalPay = order.TotalPay;
                existingOrder.OrderDetails = order.OrderDetails;

                _dbContext.SaveChanges(); // reversal + header update persist করা

                // --- Step 2: নতুন order details এর stock effect apply করো ---
                foreach (var newDetail in existingOrder.OrderDetails)
                {
                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == newDetail.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock info not found for product {newDetail.ProductId}");

                    if (stock.LoadedQty < newDetail.Quantity)
                        throw new Exception($"Insufficient loaded cylinder stock for product {newDetail.ProductId}");

                    ApplyStockForSale(stock, newDetail);
                }

                _dbContext.SaveChanges();

                // Update transaction history
                if (order.TotalPay > 0)
                {
                    var existingHistory = _dbContext.TransactionHistories
                        .FirstOrDefault(x => x.OrderId == order.Id && !x.IsDeleted);

                    var lastBalance = _dbContext.TransactionHistories
                        .Where(x => !x.IsDeleted && (existingHistory == null || x.Id != existingHistory.Id))
                        .OrderByDescending(x => x.Id)
                        .Select(x => x.CurrentBalance)
                        .FirstOrDefault() ?? 0;

                    if (existingHistory != null)
                    {
                        existingHistory.BalanceIn = order.TotalPay;
                        existingHistory.BalanceOut = 0;
                        existingHistory.CurrentBalance = lastBalance + order.TotalPay;
                        existingHistory.Date = order.Date;
                        existingHistory.Resone = "Sales Product Updated";
                    }
                    else
                    {
                        var history = new TransactionHistory
                        {
                            BalanceIn = order.TotalPay,
                            BalanceOut = 0,
                            CurrentBalance = lastBalance + order.TotalPay,
                            Date = order.Date,
                            OrderId = order.Id,
                            Resone = "Sales Product"
                        };
                        _dbContext.TransactionHistories.Add(history);
                    }

                    _dbContext.SaveChanges();
                }
                else if (order.TotalPay == 0)
                {
                    var existingHistory = _dbContext.TransactionHistories
                        .FirstOrDefault(x => x.OrderId == order.Id && !x.IsDeleted);

                    if (existingHistory != null)
                    {
                        _dbContext.TransactionHistories.Remove(existingHistory);
                        _dbContext.SaveChanges();
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        // --- Helper methods ---

        private void ApplyStockForSale(ProductStock stock, OrderDetail detail)
        {
            // Loaded cylinders sold/reduced
            stock.LoadedQty -= detail.Quantity;

            // Empty cylinders returned
            var returnQty = detail.ReturnQuantity ?? 0;
            if (returnQty > 0)
            {
                stock.EmptyQty += returnQty; // Only add what was returned
            }
        }

        private void ReverseStockForSale(ProductStock stock, OrderDetail detail)
        {
            stock.LoadedQty += detail.Quantity;

            var returnQty = detail.ReturnQuantity ?? 0;
            if (returnQty > 0)
            {
                stock.EmptyQty -= returnQty;
            }
        }

        public bool DeleteOrder(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingOrder = _dbContext.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.Id == id);

                if (existingOrder == null)
                    return false;

                if (existingOrder.IsDeleted)
                    return true; // আগে থেকেই deleted, আবার stock restore করলে double-restore হয়ে যাবে

                // Soft delete order
                existingOrder.IsDeleted = true;

                // Restore stock (sale-এর effect reverse করা)
                foreach (var item in existingOrder.OrderDetails)
                {
                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == item.ProductId);

                    if (stock != null)
                    {
                        ReverseStockForSale(stock, item);
                    }
                }

                // Soft delete transaction history
                var histories = _dbContext.TransactionHistories
                    .Where(p => p.OrderId == id && !p.IsDeleted)
                    .ToList();

                foreach (var history in histories)
                {
                    history.IsDeleted = true;
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

        public async Task<Order> GetOrderById(int orderId)
        {
            try
            {
                return await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<OrderInfo> GetOrderInfoById(int orderId)
        {
            try
            {
                return await _dbContext.Orders
                    .AsNoTracking()
                    .Where(o => o.Id == orderId)
                    .Select(o => new OrderInfo
                    {
                        OrderId = o.Id,
                        Area = o.SelectedRoad, // Assuming Area maps to DeliveryLocation
                        CustomerName = o.Customer.Name, // Ensure Customer navigation property is included
                        OrderDate = o.Date
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Optional: log the exception
                return null;
            }
        }

        public PersonInfoDTO GetPersonInfo(int personId)
        {
            try
            {
                return _dbContext.Customers
                  .Where(o => o.Id == personId)
                      .Select(o => new PersonInfoDTO
                      {
                          Id = o.Id,
                          Name = o.Name,
                          Email = o.Email,
                          Phone = o.Phone,
                      })
                  .FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IEnumerable<OrdersByPersonDTO> GetAllOrdersByPerson(int personId)
        {
            try
            {
                var orders = _dbContext.Orders
                   .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductsSize)
                   .Where(o => o.CustomerId == personId)
                   .Select(o => new OrdersByPersonDTO
                   {
                       OrderId = o.Id,
                       ProductsName = string.Join(", ", o.OrderDetails.Select(od => od.Product.DisplayNameSize)), // Comma-separated product names
                       OrderDate = o.Date,
                       Area = o.SelectedRoad,
                   })
                   .OrderByDescending(o => o.OrderId)
                    .ToList();

                return orders;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<OrdersByPersonDTO>();
            }
        }

        public async Task<IEnumerable<OrdersDTO>> GetAllOrders(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Default to last 3 days if no date is provided
                DateTime fromDate = startDate.HasValue
                    ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
                    : DateTime.Today.AddDays(-30); // default to last 30 days

                DateTime toDate = endDate.HasValue
                    ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
                    : DateTime.Today.AddDays(1).AddSeconds(-1); // today till 23:59:59


                var orders = await _dbContext.Orders
                    .Where(x => !x.IsDeleted && x.Date.Date >= fromDate && x.Date.Date <= toDate)
                    .Include(o => o.Employee)
                    .Include(o => o.OrderDetails)
                    .Include(o => o.CustomerPaymentHistories)
                    .Include(o => o.OrderPaymentHistories)
                    .Include(o => o.DailyExpenses)
                    .Include(o => o.DSRShopDues)
                    .Include(o => o.CustomerProductReturns)
                    .Include(o => o.ExtraReturnProducts)

                    .Select(o => new OrdersDTO
                    {
                        Id = o.Id,
                        Name = o.Employee.Name,
                        EmployeeId = o.Employee.Id,
                        Address = o.SelectedRoad,
                        OrderDate = o.Date,
                        TotalPrice = o.TotalAmount,

                        // Lock if any related table has data
                        IsLock = o.OrderPaymentHistories.Any(x => !x.IsDeleted) ||
                                 o.DailyExpenses.Any(x => !x.IsDeleted) ||
                                 o.ExtraReturnProducts.Any() ||
                                 o.DSRShopDues.Any(x => !x.IsDeleted) ||
                                 o.CustomerProductReturns.Any(r =>
                                    r.CustomerProductReturnDetails.Any(d => d.ReturnQuantity > 0))
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<OrdersDTO>();
            }
        }

        //public async Task<IEnumerable<DamageProductDetailsDTO>> GetDamageProductDetailsByOrderAsync(int orderId)
        //{
        //    try
        //    {
        //        return await _dbContext.DamageProductReturnDetails
        //        .Include(od => od.Product)
        //        .ThenInclude(od => od.ProductsSize)
        //        .Include(rd => rd.DamageProductReturn)
        //         .Where(rd => rd.DamageProductReturn.OrderId == orderId) // Fixed line
        //        .Select(od => new DamageProductDetailsDTO
        //        {
        //            ProductName = od.Product.DisplayNameSize,
        //            Quantity = od.Quantity,
        //            ProductPrice = od.UnitPrice,

        //            TotalPrice = od.Price
        //        })
        //        .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Enumerable.Empty<DamageProductDetailsDTO>();
        //    }
        //}

        public OrderInfoDTO OrderInfoById(int orderId)
        {
            try
            {
                OrderInfoDTO invoice = _dbContext.Orders
               .Include(o => o.Customer)
               .Where(o => o.Id == orderId)
               .Select(o => new OrderInfoDTO
               {
                   Id = o.Id,
                   Name = o.Customer.Name,
                   Email = o.Customer.Email,
                   Phone = o.Customer.Phone,

                   OrderDate = o.Date,
                   OrderId = o.Id,
                   PaymentMethod = o.PaymentMethod.Name,
                   DeliveryLocation = o.DeliveryLocation,
                   ShippingMethod = " ",
                   //ShippingMethod = o.ShippingMethod.Name,
               }).FirstOrDefault();

                return invoice;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public InvoiceDTO GetOrdersById(int orderId)
        {
            try
            {
                InvoiceDTO invoice = _dbContext.Orders
               .Include(o => o.Customer)
               .Where(o => o.Id == orderId)
               .Select(o => new InvoiceDTO
               {
                   Id = o.Id,
                   Name = o.Customer.Name,
                   Email = o.Customer.Email,
                   Phone = o.Customer.Phone,
                  

               }).FirstOrDefault();

                return invoice;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<CustomerPaymentHistoryDTO>> GetCustomerPaymentHistoryById(int customerId)
        {
            try
            {
                var customerPaymentHistory = await _dbContext.CustomerPaymentHistories
                  .Where(x => x.CustomerId == customerId && !x.IsDeleted)
                  .Select(x => new CustomerPaymentHistoryDTO
                  {
                      Id = x.Id,
                      TotalAmountThisOrder = x.TotalAmountThisOrder,
                      AmountPaid = x.AmountPaid,
                      TotalDueBeforePayment = x.TotalDueBeforePayment,
                      TotalDueAfterPayment = x.TotalDueAfterPayment,
                      PaymentDate = x.PaymentDate,
                      PaymentMethod = x.PaymentMethod.Name,
                      TransactionID = x.TransactionID,
                      Number = x.Number,
                      IsDisabled = x.OrderId.HasValue ||
                            x.CustomerProductReturnId.HasValue ||
                            x.DSRShopDueId.HasValue ||
                            x.DailyExpenseId.HasValue ||
                            x.OrderPaymentHistoryId.HasValue

                  })
                  .OrderByDescending(x => x.Id)
                  .ToListAsync();

                return customerPaymentHistory;
            }
            catch (Exception ex)
            {
                return new List<CustomerPaymentHistoryDTO>();
            }
        }

        public List<(string name, double value)> GetPayment(int id)
        {
            var order = _dbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order is not null)
            {
                return new List<(string Name, double Value)>
                {
                    ("Delivery Charge", order.DeliveryCharge),
                    ("Grand Total", order.TotalAmount),
                    ("Discount", order.Discount),
                    ("Total Pay", order.TotalPay),
                    ("Total Due", order.TotalDue)
                };
            }
            else
            {
                return new List<(string Name, double Value)>();
            }
        }

        public CustomerPaymentHistoryDTO DuePayment(int id)
        {
            try
            {
                CustomerPaymentHistoryDTO customerPaymentHistory = _dbContext.CustomerPaymentHistories
                .Where(x => x.Id == id)
                  .Select(x => new CustomerPaymentHistoryDTO
                  {
                      Id = x.Id,
                      TotalAmountThisOrder = x.TotalAmountThisOrder,
                      AmountPaid = x.AmountPaid,
                      TotalDueBeforePayment = x.TotalDueBeforePayment,
                      TotalDueAfterPayment = x.TotalDueAfterPayment,
                      PaymentDate = x.PaymentDate,
                      PaymentMethod = x.PaymentMethod.Name,
                      TransactionID = x.TransactionID,
                      Number = x.Number,
                  })
                  .FirstOrDefault();

                return customerPaymentHistory;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<ExistingOrderDTO>> GetExistingOrderById(int orderId)
        {
            return await _dbContext.OrderDetails
                    .Where(x => x.OrderId == orderId)
                    .Select(x => new ExistingOrderDTO
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        StockQty = x.Quantity
                    })
                    .ToListAsync();
        }

        public IEnumerable<OrderDetailsDTO> GetOrderDetailsByOrderId(int orderId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderDetailsDTO>> GetOrderDetailsByOrderAsync(int orderId)
        {
            try
            {
                var orderDetails = await _dbContext.OrderDetails
                    .Include(od => od.Product)
                        .ThenInclude(p => p.ProductsSize)
                    .Where(od => od.OrderId == orderId)
                    .Select(od => new OrderDetailsDTO
                    {
                        ProductId = od.ProductId,
                        ProductName = od.Product.DisplayNameSize ?? od.Product.Name,
                        Quantity = od.Quantity,
                        ReturnQuantity = od.ReturnQuantity ?? 0,
                        SellingQuantity = od.Quantity - (od.ReturnQuantity ?? 0),
                        ProductPrice = od.CylinderUnitPrice + od.GasUnitPrice,
                        TotalProductPrice = od.Quantity * (od.CylinderUnitPrice + od.GasUnitPrice),
                        ReturnPrice = (od.ReturnQuantity ?? 0) * (od.CylinderUnitPrice + od.GasUnitPrice),
                        Discount = od.Discount,
                        TotalPrice = (od.Quantity - (od.ReturnQuantity ?? 0)) * (od.CylinderUnitPrice + od.GasUnitPrice) - od.Discount,

                        // New fields
                        CylinderUnitPrice = od.CylinderUnitPrice,
                        GasUnitPrice = od.GasUnitPrice,
                        TotalCylinderPrice = od.Quantity * od.CylinderUnitPrice,
                        TotalGasPrice = od.Quantity * od.GasUnitPrice
                    })
                    .ToListAsync();

                return orderDetails;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error getting order details for order {OrderId}", orderId);
                return Enumerable.Empty<OrderDetailsDTO>();
            }
        }

        public IEnumerable<CustomerDueDTO> GetCustomerDueHistory()
        {
            try
            {
                var query = from customer in _dbContext.Employees
                            join payment in _dbContext.CustomerPaymentHistories.Where(p => !p.IsDeleted)
                                on customer.Id equals payment.EmployeeId into paymentsGroup
                            select new CustomerDueDTO
                            {
                                Id = customer.Id,
                                Name = customer.Name,
                                Phone = customer.Phone,
                                District = customer.Address,
                                TotalDue = paymentsGroup.OrderByDescending(p => p.Id)
                                                        .Select(p => (double?)p.TotalDueAfterPayment)
                                                        .FirstOrDefault() ?? 0
                            };

                return query.Where(c => c.TotalDue != 0)
                            .OrderByDescending(c => c.TotalDue)
                            .ToList();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<CustomerDueDTO>();
            }
        }
    }
}