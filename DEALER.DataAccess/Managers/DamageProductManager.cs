//namespace DEALER.DataAccess
//{
//    public class DamageProductManager : BaseDataManager, IDamageProduct
//    {
//        public DamageProductManager(DEALERContext model) : base(model)
//        {
//        }

//        public int CreateDamageProduct(DamageProduct DamageProduct)
//        {
//            AddUpdateEntity(DamageProduct);
//            return DamageProduct.Id;
//        }

//        public bool UpdateDamageProduct(DamageProduct DamageProduct)
//        {
//            return AddUpdateEntity(DamageProduct);
//        }

//        public bool ReceivedDamageProduct(int id)
//        {
//            try
//            {
//                var entity = GetDamageProduct(id);
//                entity.IsReceived = true;
//                _dbContext.SaveChanges();

//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        public bool DeleteDamageProduct(int id)
//        {
//            try
//            {
//                return RemoveEntity<DamageProduct>(id);
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        public DamageProduct GetDamageProduct(int id)
//        {
//            try
//            {
//                return _dbContext.DamageProducts
//                    .FirstOrDefault(c => c.Id == id);
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }

//        public async Task<IList<DamageProduct>> GetAllDamageProduct(int supplierId, DateTime? startDate, DateTime? endDate)
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


//                return await _dbContext.DamageProducts
//                    .Include(x => x.Customer)
//                    .Include(x => x.Product)
//                     .ThenInclude(p => p.ProductsSize)
//                    .Include(x => x.Product)
//                        .ThenInclude(p => p.Supplier) // <-- Add this line
//                        .Where(x => !x.IsReceived && x.Date.Date >= fromDate && x.Date.Date <= toDate && x.Product.SupplierId == supplierId)
//                    .OrderByDescending(x => x.Id)
//                    .ToListAsync();
//            }
//            catch (Exception ex)
//            {
//                return new List<DamageProduct>();
//            }
//        }

//        public async Task<IList<DamageProductSummaryDTO>> GetAllDamageProductInStock(int supplierId)
//        {
//            try
//            {
//                var result = _dbContext.DamageProducts
//                         .Where(x => !x.IsReceived && x.Product.SupplierId == supplierId)
//                         .Include(x => x.Product)
//                             .ThenInclude(p => p.ProductsSize)
//                         .Include(x => x.Product)
//                             .ThenInclude(p => p.Supplier)
//                         .AsEnumerable()
//                         .GroupBy(x => new
//                         {
//                             x.ProductId,
//                             ProductName = $"{x.Product.DisplayNameSize}",
//                             SupplierName = x.Product.Supplier.Name
//                         })
//                         .Select(g => new DamageProductSummaryDTO
//                         {
//                             ProductName = g.Key.ProductName,
//                             SupplierName = g.Key.SupplierName,
//                             TotalQuantity = g.Sum(x => x.Quantity),
//                             TotalPrice = g.Sum(x => x.UnitPrice * x.Quantity),
//                         })
//                         .ToList(); // You can use ToList() if AsEnumerable() already moved execution to client


//                return result;
//            }
//            catch (Exception ex)
//            {
//                return new List<DamageProductSummaryDTO>();
//            }
//        }

//        public async Task<IList<DamageStockSummaryDTO>> GetAllDamageProductInStockBySupplier()
//        {
//            try
//            {
//                var result = await _dbContext.DamageProducts
//                    .Where(x => !x.IsReceived && !x.Product.Supplier.IsDisable)
//                    .Include(x => x.Product)
//                        .ThenInclude(p => p.Supplier)
//                    .GroupBy(x => new
//                    {
//                        x.Product.Supplier.Id,
//                        x.Product.Supplier.Name
//                    })
//                    .Select(g => new DamageStockSummaryDTO
//                    {
//                        SupplierId = g.Key.Id,
//                        SupplierName = g.Key.Name,
//                        StockQuantity = g.Sum(x => x.Quantity),
//                        StockAmount = g.Sum(x => x.UnitPrice * x.Quantity)
//                    })
//                    .OrderBy(x => x.SupplierName)
//                    .ToListAsync();

//                return result;
//            }
//            catch (Exception ex)
//            {
//                // Consider logging the exception here
//                return new List<DamageStockSummaryDTO>();
//            }
//        }
//    }
//}
