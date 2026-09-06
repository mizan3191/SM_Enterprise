//namespace DEALER.DataAccess
//{
//    public class DamageProductHandoverManager : BaseDataManager, IDamageProductHandover
//    {
//        public DamageProductHandoverManager(DEALERContext model) : base(model)
//        {
//        }

//        public bool DeleteDamageProductHandover(int id)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();

//            try
//            {
//                // Find the handover including its details
//                var handover = _dbContext.DamageProductHandovers
//                    .Include(h => h.DamageProductHandoverDetails)
//                    .FirstOrDefault(h => h.Id == id);

//                if (handover == null)
//                {
//                    return false;
//                }

//                // First, update all related damage products to set IsReceived = false
//                foreach (var detail in handover.DamageProductHandoverDetails)
//                {
//                    var damageIdList = detail.DamageReturnIdList
//                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
//                        .Select(x => int.Parse(x.Trim()))
//                        .ToList();

//                    foreach (int damageId in damageIdList)
//                    {
//                        var damageProduct = _dbContext.DamageProducts.FirstOrDefault(x => x.Id == damageId);
//                        if (damageProduct != null)
//                        {
//                            damageProduct.IsReceived = false;
//                            _dbContext.Update(damageProduct);
//                        }
//                    }
//                }

//                // Then delete the handover and its details
//                _dbContext.Remove(handover);
//                _dbContext.SaveChanges();

//                transaction.Commit();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                transaction.Rollback();
//                return false;
//            }
//        }

//        public bool UpdateDamageProductHandover(DamageProductHandover DamageProductHandover)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();

//            try
//            {
//                // Update the main handover record
//                _dbContext.Update(DamageProductHandover);
//                _dbContext.SaveChanges();

//                // Process details and update related damage products
//                foreach (var Detail in DamageProductHandover.DamageProductHandoverDetails)
//                {
//                    var damageIdList = Detail.DamageReturnIdList
//                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
//                        .Select(x => int.Parse(x.Trim()))
//                        .ToList();

//                    foreach (int id in damageIdList)
//                    {
//                        var damageProductEntity = _dbContext.DamageProducts.FirstOrDefault(x => x.Id == id);
//                        if (damageProductEntity != null)
//                        {
//                            damageProductEntity.IsReceived = true;
//                            _dbContext.Update(damageProductEntity);
//                        }
//                    }
//                }

//                _dbContext.SaveChanges();
//                transaction.Commit();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                transaction.Rollback();
//                return false;
//            }
//        }

//        public int CreateDamageProductHandover(DamageProductHandover DamageProductHandover)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();

//            try
//            {
//                DamageProductHandover.Id = 0;

//                _dbContext.Add(DamageProductHandover);
//                _dbContext.SaveChanges();

//                var DamageProductHandoverId = DamageProductHandover.Id;
//                var Details = DamageProductHandover.DamageProductHandoverDetails;

//                foreach (var Detail in Details)
//                {
//                    var damageIdList = Detail.DamageReturnIdList
//                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
//                        .Select(x => int.Parse(x.Trim()))
//                        .ToList();

//                    foreach (int id in damageIdList)
//                    {
//                        var damageProductEntity = _dbContext.DamageProducts.FirstOrDefault(x => x.Id == id);
//                        if (damageProductEntity != null)
//                        {
//                            damageProductEntity.IsReceived = true;
//                            //AddUpdateEntity(damageProductEntity);
//                            _dbContext.Update(Detail);
//                            _dbContext.SaveChanges();
//                        }
//                    }
//                }

//                _dbContext.SaveChanges();
//                transaction.Commit();
//                return DamageProductHandover.Id;
//            }
//            catch (Exception ex)
//            {
//                transaction.Rollback();
//                return 0;
//            }
//        }


//        public IList<DamageProductHandoverDTO> GetDamageProductBySupplierId(int supplierId)
//        {
//            try
//            {
//                var result = _dbContext.DamageProducts
//                    .Include(x => x.Product)
//                    .ThenInclude(x => x.ProductsSize)
//                    .Where(x => !x.IsReceived && x.Product.SupplierId == supplierId)
//                    .AsEnumerable() // Grouping needs in-memory to access navigation properties
//                    .GroupBy(x => new { x.ProductId, x.UnitPrice }) // group by Product and UnitPrice
//                    .Select(g => new DamageProductHandoverDTO
//                    {
//                        Id = g.First().Id,
//                        ProductId = g.First().Product.Id,
//                        DamageReturnIdList = string.Join(",", g.Select(x => x.Id)),
//                        Products = g.First().Product.DisplayNameSize, // you can also concatenate names if needed
//                        Quantity = g.Sum(x => x.Quantity),
//                        UnitPrice = g.Key.UnitPrice,
//                        TotalPrice = g.Sum(x => x.Quantity * x.UnitPrice)
//                    })
//                    .ToList();

//                return result;
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }

//        public async Task<IList<DamageProductHandover>> GetAllDamageProductHandoverList(int supplierId, DateTime? startDate, DateTime? endDate)
//        {
//            try
//            {

//                // Default to last 3 days if no date is provided
//                DateTime fromDate = startDate.HasValue
//                    ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
//                    : DateTime.Today.AddDays(-30); // default to last 30 days

//                DateTime toDate = endDate.HasValue
//                    ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
//                    : DateTime.Today.AddDays(1).AddSeconds(-1); // today till 23:59:59

//                return await _dbContext.DamageProductHandovers
//                    .Include(x => x.Customer)
//                    .Include(x => x.Supplier)
//                    .Where(x => x.Date.Date >= fromDate && x.Date.Date <= toDate && x.SupplierId == supplierId)
//                    .OrderByDescending(x => x.Id)
//                    .ToListAsync();
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }


//        public async Task<IList<DamageProductHandoverSummary>> GetTotalAmountSupplierWiseAsync()
//        {
//            try
//            {
//                // Get total due per supplier
//                var dueAmounts = await _dbContext.DamageProductHandovers
//                    .Where(x => !x.Supplier.IsDisable)
//                    .GroupBy(x => new { x.SupplierId, x.Supplier.Name })
//                    .Select(g => new
//                    {
//                        g.Key.SupplierId,
//                        SupplierName = g.Key.Name,
//                        TotalDue = g.Sum(x => x.TotalPrice)
//                    })
//                    .ToListAsync();

//                // Get total paid per supplier
//                var paidAmounts = await _dbContext.DamageProductHandoverPaymentHistories
//                    .Where(x => !x.IsDeleted)
//                    .GroupBy(x => new { x.SupplierId, x.Supplier.Name })
//                    .Select(g => new
//                    {
//                        g.Key.SupplierId,
//                        TotalPaid = g.Sum(x => x.AmountPaid)
//                    })
//                    .ToListAsync();

//                // Merge due and paid amounts
//                var result = dueAmounts.Select(d => new DamageProductHandoverSummary
//                {
//                    SupplierId = d.SupplierId,
//                    SupplierName = d.SupplierName,
//                    TotalDueAmount = d.TotalDue - (paidAmounts.FirstOrDefault(p => p.SupplierId == d.SupplierId)?.TotalPaid ?? 0)
//                }).ToList();

//                return result;
//            }
//            catch (Exception ex)
//            {
//                // Optionally log the exception
//                return new List<DamageProductHandoverSummary>();
//            }
//        }


//        public async Task<IList<DamageProductHandoverDuePaymentSummary>> GetDamageProductHandoverDuePaymentSummary(int supplierId)
//        {
//            try
//            {
//                // Get due entries (DamageProductHandover records)
//                var dues = await _dbContext.DamageProductHandovers
//                    .Where(x => x.SupplierId == supplierId && !x.IsDeleted)
//                    .Select(x => new
//                    {
//                        x.SupplierId,
//                        SupplierName = x.Supplier.Name,
//                        Date = x.Date.Date,
//                        DueAmount = x.TotalPrice - x.ExtraPrice - x.DiscountPrice
//                    })
//                    .ToListAsync();

//                // Get payment entries
//                var payments = await _dbContext.DamageProductHandoverPaymentHistories
//                    .Where(x => x.SupplierId == supplierId && !x.IsDeleted)
//                    .Select(x => new
//                    {
//                        x.SupplierId,
//                        SupplierName = x.Supplier.Name,
//                        Date = x.Date.Date,
//                        PaidAmount = x.AmountPaid
//                    })
//                    .ToListAsync();

//                // Group and combine data
//                var allData = dues
//                    .GroupBy(x => x.Date)
//                    .Select(g => new DamageProductHandoverTempDuePaymentSummary
//                    {
//                        Date = g.Key,
//                        SupplierId = g.First().SupplierId,
//                        SupplierName = g.First().SupplierName,
//                        DueAmount = g.Sum(x => x.DueAmount),
//                        PaidAmount = 0
//                    })
//                    .ToList();

//                foreach (var paymentGroup in payments.GroupBy(x => x.Date))
//                {
//                    var existing = allData.FirstOrDefault(x => x.Date == paymentGroup.Key);
//                    if (existing != null)
//                    {
//                        existing.PaidAmount = paymentGroup.Sum(x => x.PaidAmount);
//                    }
//                    else
//                    {
//                        allData.Add(new DamageProductHandoverTempDuePaymentSummary
//                        {
//                            Date = paymentGroup.Key,
//                            SupplierId = paymentGroup.First().SupplierId,
//                            SupplierName = paymentGroup.First().SupplierName,
//                            DueAmount = 0,
//                            PaidAmount = paymentGroup.Sum(x => x.PaidAmount)
//                        });
//                    }
//                }

//                // Project to final result
//                var result = allData
//                    .OrderByDescending(x => x.Date)
//                    .Select(x => new DamageProductHandoverDuePaymentSummary
//                    {
//                        SupplierId = x.SupplierId,
//                        SupplierName = x.SupplierName,
//                        Date = x.Date,
//                        DueAmount = x.DueAmount,
//                        PaidAmount = x.PaidAmount
//                    })
//                    .ToList();

//                return result;
//            }
//            catch (Exception)
//            {
//                return new List<DamageProductHandoverDuePaymentSummary>();
//            }
//        }

//        public DamageProductHandover GetDamageProductHandoverById(int handoverId)
//        {
//            return _dbContext.DamageProductHandovers
//                .Include(h => h.DamageProductHandoverDetails)
//                .ThenInclude(p => p.Product)
//                .Include(h => h.Customer)
//                .Include(h => h.Supplier)
//                .FirstOrDefault(h => h.Id == handoverId);
//        }

//        public IList<DamageProductHandoverListDetailsDTO> GetAllDamageProductHandoverListDetails(int handoverId)
//        {
//            return _dbContext.DamageProductHandoverDetails
//                .Where(d => d.DamageProductHandoverId == handoverId)
//                .Include(d => d.Product)
//                .ThenInclude(d => d.ProductsSize)
//                .GroupBy(d => d.ProductId)
//                .Select(g => new DamageProductHandoverListDetailsDTO
//                {
//                    Id = g.First().Id,
//                    ProductName = g.First().Product.DisplayNameSize,
//                    Quantity = g.Sum(d => d.Quantity),
//                    TotalPrice = g.Sum(d => d.TotalPrice),
//                })
//                .ToList();
//        }
//    }
//}