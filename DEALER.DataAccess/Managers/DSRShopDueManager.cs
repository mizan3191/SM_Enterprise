namespace DEALER.DataAccess
{
    public class DSRShopDueManager : BaseDataManager, IDSRShopDue
    {
        public DSRShopDueManager(DEALERContext model) : base(model)
        {
        }

        // সম্পূর্ণ ডেটা সেভ করার জন্য (প্রোডাক্ট সহ)
        public int CreateDSRShopDue(DSRShopDue DSRShopDue)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                DSRShopDue.Id = 0;

                // Products আলাদাভাবে সেভ করার জন্য রাখছি
                var products = DSRShopDue.Products?.ToList() ?? new List<ShopEmptyCylinderProduct>();
                DSRShopDue.Products = new List<ShopEmptyCylinderProduct>(); // খালি রাখো

                AddUpdateEntity(DSRShopDue);
                _dbContext.SaveChanges(); // DSRShopDue.Id generate হবে

                // Products সেভ করো (যদি থাকে)
                if (products.Any())
                {
                    foreach (var product in products)
                    {
                        product.DSRShopDueId = DSRShopDue.Id;
                        product.Id = 0;
                        _dbContext.ShopEmptyCylinderProducts.Add(product);
                    }
                    _dbContext.SaveChanges();
                }

                // Customer Payment History
                if (DSRShopDue.OrderId > 0)
                {
                    var lastPayment = _dbContext.CustomerPaymentHistories
                                    .Where(p => p.EmployeeId == DSRShopDue.DSREmployeeId)
                                    .OrderByDescending(p => p.Id)
                                    .FirstOrDefault();

                    double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0;
                    double totalDueAfter = totalDueBefore + DSRShopDue.DueAmount;

                    var payment = new CustomerPaymentHistory()
                    {
                        EmployeeId = DSRShopDue.DSREmployeeId.Value,
                        OrderId = DSRShopDue.OrderId,
                        DSRShopDueId = DSRShopDue.Id,
                        PaymentDate = DSRShopDue.Date,
                        PaymentMethodId = 1,
                        TransactionID = string.Empty,
                        Number = string.Empty,
                        TotalAmountThisOrder = 0,
                        AmountPaid = DSRShopDue.DueAmount,
                        TotalDueBeforePayment = totalDueBefore,
                        TotalDueAfterPayment = totalDueAfter
                    };

                    _dbContext.CustomerPaymentHistories.Add(payment);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return DSRShopDue.Id;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }


        public bool UpdateDSRShopDue(DSRShopDue DSRShopDue)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                // ✅ FIX: Detach any already-tracked DSRShopDue to avoid EF conflict
                var trackedEntry = _dbContext.ChangeTracker
                    .Entries<DSRShopDue>()
                    .FirstOrDefault(e => e.Entity.Id == DSRShopDue.Id);
                if (trackedEntry != null)
                    trackedEntry.State = EntityState.Detached;
                // ✅ FIX: Detach tracked products too
                var trackedProducts = _dbContext.ChangeTracker
                    .Entries<ShopEmptyCylinderProduct>()
                    .Where(e => e.Entity.DSRShopDueId == DSRShopDue.Id)
                    .ToList();
                foreach (var p in trackedProducts)
                    p.State = EntityState.Detached;
                // Now load cleanly from DB
                var existing = _dbContext.DSRShopDues
                    .Include(d => d.Products)
                    .FirstOrDefault(d => d.Id == DSRShopDue.Id);
                if (existing == null) return false;
                var previousAmount = existing.DueAmount;
                // Update main properties
                existing.EmployeeId = DSRShopDue.EmployeeId;
                existing.ShopId = DSRShopDue.ShopId;
                existing.DueAmount = DSRShopDue.DueAmount;
                existing.Date = DSRShopDue.Date;
                existing.IsDeleted = DSRShopDue.IsDeleted;
                // ✅ NOTE: Products এখন স্বাধীনভাবে AddShopEmptyCylinderProduct দিয়ে save হয়,
                //          তাই এখানে শুধু main entity update করলেই হবে।
                //          কিন্তু যদি HandleSubmit থেকে কোনো unsaved (Id==0) product আসে,
                //          তাহলে সেটাও handle করো:
                var incomingProducts = DSRShopDue.Products ?? new List<ShopEmptyCylinderProduct>();
                var incomingIds = incomingProducts
                    .Where(p => p.Id > 0)
                    .Select(p => p.Id)
                    .ToHashSet();
                // Remove products that are no longer in the list
                var toRemove = existing.Products
                    .Where(ep => !incomingIds.Contains(ep.Id))
                    .ToList();
                if (toRemove.Any())
                    _dbContext.ShopEmptyCylinderProducts.RemoveRange(toRemove);
                // Add any truly new product (Id == 0) that HandleSubmit sent
                foreach (var product in incomingProducts.Where(p => p.Id <= 0))
                {
                    product.Id = 0;
                    product.DSRShopDueId = DSRShopDue.Id;
                    _dbContext.ShopEmptyCylinderProducts.Add(product);
                }
                _dbContext.SaveChanges();
                // Update Customer Payment History
                if (DSRShopDue.OrderId > 0 && DSRShopDue.DSREmployeeId.HasValue)
                {
                    var payment = _dbContext.CustomerPaymentHistories
                        .FirstOrDefault(p => p.DSRShopDueId == DSRShopDue.Id
                                          && p.EmployeeId == DSRShopDue.DSREmployeeId);
                    if (payment != null)
                    {
                        double diff = DSRShopDue.DueAmount - previousAmount;
                        payment.PaymentDate = DSRShopDue.Date;
                        payment.AmountPaid = DSRShopDue.DueAmount;
                        payment.TotalDueAfterPayment += diff;
                        _dbContext.Update(payment);
                        _dbContext.SaveChanges();
                        if (diff != 0)
                            RecalculateCustomerPayments(payment.EmployeeId.Value, payment.Id, diff);
                    }
                    else
                    {
                        var lastPayment = _dbContext.CustomerPaymentHistories
                            .Where(p => p.EmployeeId == DSRShopDue.DSREmployeeId)
                            .OrderByDescending(p => p.Id)
                            .FirstOrDefault();
                        double before = lastPayment?.TotalDueAfterPayment ?? 0;
                        _dbContext.CustomerPaymentHistories.Add(new CustomerPaymentHistory
                        {
                            EmployeeId = DSRShopDue.DSREmployeeId.Value,
                            OrderId = DSRShopDue.OrderId,
                            DSRShopDueId = DSRShopDue.Id,
                            PaymentDate = DSRShopDue.Date,
                            PaymentMethodId = 1,
                            TransactionID = string.Empty,
                            Number = string.Empty,
                            TotalAmountThisOrder = 0,
                            AmountPaid = DSRShopDue.DueAmount,
                            TotalDueBeforePayment = before,
                            TotalDueAfterPayment = before + DSRShopDue.DueAmount
                        });
                        _dbContext.SaveChanges();
                    }
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


        /// <summary>
        /// Add products for a specific DSRShopDue
        /// </summary>
        public bool AddShopEmptyCylinderProduct(ShopEmptyCylinderProduct product)
        {
            product.Id = 0;                     // নিশ্চিত করা হচ্ছে নতুন insert হবে
            return AddUpdateEntity(product);
        }

        /// <summary>
        /// Single product row delete করার জন্য
        /// </summary>
        public bool DeleteShopEmptyCylinderProduct(int productId)
        {
            var product = _dbContext.ShopEmptyCylinderProducts.Find(productId);
            if (product == null) return false;
            _dbContext.ShopEmptyCylinderProducts.Remove(product);
            _dbContext.SaveChanges();
            return true;
        }


        public bool DeleteShopDue(int id)
        {
            try
            {
                using var transaction = _dbContext.Database.BeginTransaction();

                var shopDue = _dbContext.DSRShopDues
                    .Include(d => d.Products)
                    .FirstOrDefault(c => c.Id == id);

                if (shopDue == null)
                    return false;

                // Soft delete main entity
                shopDue.IsDeleted = true;
                _dbContext.Update(shopDue);
                _dbContext.SaveChanges();

                // Soft delete products
                if (shopDue.Products != null && shopDue.Products.Any())
                {
                    foreach (var product in shopDue.Products)
                    {
                        // If you have IsDeleted in ShopEmptyCylinderProduct, set it true
                        // Otherwise, you can physically remove them
                        _dbContext.ShopEmptyCylinderProducts.Remove(product);
                    }
                    _dbContext.SaveChanges();
                }

                // Update Customer Payment History
                if (shopDue.OrderId > 0 && shopDue.OrderId is not null)
                {
                    var payment = _dbContext.CustomerPaymentHistories
                                 .FirstOrDefault(p => p.EmployeeId == shopDue.DSREmployeeId
                                 && p.OrderId == shopDue.OrderId
                                 && p.DSRShopDueId == shopDue.Id);

                    if (payment != null)
                    {
                        payment.IsDeleted = true;
                        _dbContext.Update(payment);
                        _dbContext.SaveChanges();

                        // Recalculate subsequent payments (subtract the amount)
                        RecalculateCustomerPayments(payment.EmployeeId.Value, payment.Id, -payment.AmountPaid);
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void RecalculateCustomerPayments(int employeeId, int id, double amount)
        {
            if (amount == 0)
                return;

            try
            {
                var payments = _dbContext.CustomerPaymentHistories
                                   .Where(p => p.EmployeeId == employeeId && p.Id > id)
                                   .OrderBy(p => p.Id)
                                   .ToList();

                if (!payments.Any())
                {
                    return;
                }

                foreach (var payment in payments)
                {
                    payment.TotalDueBeforePayment += amount;
                    payment.TotalDueAfterPayment += amount;
                }

                _dbContext.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public IList<DSRShopDueForOrderDTO> GetAllDSRShopDueByOrderId(int orderId)
        {
            try
            {
                return _dbContext.DSRShopDues
                    .Include(x => x.Employee)
                    .Include(x => x.DSREmployee)
                    .Include(x => x.Shop)
                    .Include(x => x.Products)
                        .ThenInclude(p => p.Product)
                    .Where(x => x.OrderId == orderId && !x.IsDeleted && !x.Shop.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new DSRShopDueForOrderDTO
                    {
                        Id = x.Id,
                        EmployeeName = x.Employee != null ? x.Employee.Name : "",
                        DSRCustomerName = x.DSREmployee != null ? x.DSREmployee.Name : "",
                        ShopName = x.Shop != null ? x.Shop.Name : "",
                        OrderId = x.OrderId,
                        DueAmount = x.DueAmount,
                        Date = x.Date,
                        TotalCylinderQty = x.Products != null ? x.Products.Sum(p => p.CylinderQty) : 0,
                        Products = x.Products != null ? x.Products.Select(p => new ShopEmptyCylinderProductDTO
                        {
                            Id = p.Id,
                            ProductId = p.ProductId,
                            ProductName = p.Product != null ? p.Product.Name : "",
                            CylinderQty = p.CylinderQty
                        }).ToList() : new List<ShopEmptyCylinderProductDTO>()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<DSRShopDueForOrderDTO>();
            }
        }

        public DSRShopDue GetDSRShopDue(int id)
        {
            try
            {
                return _dbContext.DSRShopDues
                    .Include(d => d.Products)
                        .ThenInclude(p => p.Product)
                    .Include(d => d.Employee)
                    .Include(d => d.Shop)
                    .FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<Lov>> GetAllShopDueCustomerList(int shopId)
        {
            try
            {
                var list = await _dbContext.DSRShopDues
                            .Include(x => x.Employee)
                            .Where(x => !x.IsDeleted
                                && !x.Shop.IsDeleted
                                && !x.Employee.IsDisable
                                && x.ShopId == shopId)
                            .Select(x => new Lov
                            {
                                Id = x.EmployeeId.Value,
                                Name = x.Employee.Name,
                            })
                            .Distinct()
                            .OrderBy(x => x.Name)
                            .ToListAsync();

                return list;
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public IList<DSRShopDue> GetAllDSRShopDue()
        {
            try
            {
                return _dbContext.DSRShopDues
                    .Include(x => x.Employee)
                    .Include(x => x.DSREmployee)
                    .Include(x => x.Shop)
                    .Include(x => x.Products)
                        .ThenInclude(p => p.Product)
                    .Where(x => !x.IsDeleted && !x.Shop.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<DSRShopDue>();
            }
        }

        public IList<DSRShopDueDTO> GetAllDSRShopDueList(DateTime? startDate, DateTime? endDate)
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


                return _dbContext.DSRShopDues
                    .Include(x => x.Employee)
                    .Include(x => x.Shop)
                    .Where(x => !x.IsDeleted && !x.Shop.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new DSRShopDueDTO
                    {
                        Id = x.Id,
                        EmployeeName = x.Employee.Name,
                        ShopName = x.Shop.Name,
                        OrderId = x.OrderId,
                        DueAmount = x.DueAmount,
                        ShopArea = x.Shop.Area,
                        ShopNumber = x.Shop.Number,
                        ShopShopOwner = x.Shop.ShopOwner,
                        Date = x.Date
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // Optional: Log the error if needed
                return new List<DSRShopDueDTO>();
            }
        }

        public IList<DSRShopDueDTO> GetAllDSRShopDueList(int shopId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Default to last 3 days if no date is provided
                DateTime fromDate = startDate.HasValue
                    ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
                    : DateTime.Today.AddDays(-30); // default to last 7 days

                DateTime toDate = endDate.HasValue
                    ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
                    : DateTime.Today.AddDays(1).AddSeconds(-1); // today till 23:59:59


                return _dbContext.DSRShopDues
                    .Include(x => x.Employee)
                    .Include(x => x.DSREmployee)
                    .Include(x => x.Shop)
                    .Where(x => x.Date.Date >= fromDate && x.Date.Date <= toDate && !x.IsDeleted && x.ShopId == shopId && !x.Shop.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new DSRShopDueDTO
                    {
                        Id = x.Id,
                        EmployeeName = x.Employee.Name,
                        IssuedBYCustomerName = x.DSREmployee.Name,
                        ShopName = x.Shop.Name,
                        OrderId = x.OrderId,
                        DueAmount = x.DueAmount,
                        CylinderDueAmount = x.Products.Sum(p => p.DueAmount),
                        ShopArea = x.Shop.Area,
                        ShopNumber = x.Shop.Number,
                        ShopShopOwner = x.Shop.ShopOwner,
                        Date = x.Date
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // Optional: Log the error if needed
                return new List<DSRShopDueDTO>();
            }
        }

        public async Task<IList<DSRShopDue>> GetAllDSRShopDue(int orderId)
        {
            try
            {
                return await _dbContext.DSRShopDues
                    .Include(x => x.Employee)
                    .Include(x => x.DSREmployee)
                    .Include(x => x.Shop)
                    .Where(x => x.OrderId == orderId && !x.IsDeleted && !x.Shop.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<DSRShopDue>();
            }
        }

        public async Task<double> LoadShopDueCustomerWise(int shopId, int employeeId)
        {
            try
            {
                var dueQuery = _dbContext.DSRShopDues
                    .Where(x => !x.IsDeleted &&
                               !x.Shop.IsDeleted &&
                               x.ShopId == shopId &&
                               x.EmployeeId == employeeId)
                    .Select(x => x.DueAmount);

                var paymentQuery = _dbContext.DSRShopPaymentHistories
                    .Where(x => !x.IsDeleted &&
                               x.ShopId == shopId &&
                               x.EmployeeId == employeeId)
                    .Select(x => x.AmountPaid);

                var totalDue = await dueQuery.SumAsync();
                var totalPaid = await paymentQuery.SumAsync();

                var remainingDue = totalDue - totalPaid;
                return Math.Max(0, remainingDue);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<double> LoadShopCylinderDueCustomerWise(int shopId, int employeeId)
        {
            try
            {
                var dueQuery = _dbContext.DSRShopDues
                    .Where(x => !x.IsDeleted &&
                               !x.Shop.IsDeleted &&
                               x.ShopId == shopId &&
                               x.EmployeeId == employeeId)
                    .Select(x => x.DueAmount);

                var paymentQuery = _dbContext.DSRShopPaymentHistories
                    .Where(x => !x.IsDeleted &&
                               x.ShopId == shopId &&
                               x.EmployeeId == employeeId)
                    .Select(x => x.AmountPaid);

                var totalDue = await dueQuery.SumAsync();
                var totalPaid = await paymentQuery.SumAsync();

                var remainingDue = totalDue - totalPaid;
                return Math.Max(0, remainingDue);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<IEnumerable<ShopDueWithCylinderDto>> GetShopDueWithCylindersAsync(int orderId)
        {
            var result = await _dbContext.DSRShopDues
                .Include(d => d.Shop)
                .Include(d => d.Employee)
                .Include(d => d.Products)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(p => p.Supplier)
                .Include(d => d.Products)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(p => p.CurrentPrice)
                .Where(d => d.OrderId == orderId && !d.IsDeleted)
                .Select(d => new ShopDueWithCylinderDto
                {
                    ShopId = d.ShopId,
                    ShopName = d.Shop.Name,
                    ShopArea = d.Shop.Area,
                    CustomerName = d.Employee != null ? d.Employee.Name : "",
                    DueAmount = d.DueAmount,
                    Date = d.Date,
                    CylinderDetails = d.Products.Select(p => new ShopCylinderDetailDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Product.DisplayNameSize ?? p.Product.Name,
                        SupplierId = p.Product.SupplierId,
                        SupplierName = p.Product.Supplier.Name,
                        Quantity = p.CylinderQty,
                        EmptyCylinderPrice = p.Product.CurrentPrice != null ? p.Product.CurrentPrice.CylinderSellingPrice : 0
                    }).ToList()
                })
                .OrderBy(d => d.ShopName)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<ShopCylinderGroupDto>> GetShopCylindersGroupedBySupplierAsync(int orderId)
        {
            var shopDues = await GetShopDueWithCylindersAsync(orderId);

            // Collect all cylinders from all shops
            var allCylinders = shopDues.SelectMany(d => d.CylinderDetails).ToList();

            // Group by Supplier
            var grouped = allCylinders
                .GroupBy(c => new { c.SupplierId, c.SupplierName })
                .Select(g => new ShopCylinderGroupDto
                {
                    SupplierId = g.Key.SupplierId,
                    SupplierName = g.Key.SupplierName,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    Details = g.ToList()
                })
                .OrderBy(g => g.SupplierName)
                .ToList();

            return grouped;
        }
    }
}