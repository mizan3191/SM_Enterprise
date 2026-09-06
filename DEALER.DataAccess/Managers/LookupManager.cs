using DEALER.Domain;
using System.Drawing;

namespace DEALER.DataAccess
{
    public class LookupManager : BaseDataManager, ILookup
    {
        public LookupManager(DEALERContext model) : base(model)
        {
        }

        public DateTime GetOrderDate(int orderId)
        {
            return _dbContext.Orders.FirstOrDefault(x => x.Id == orderId).Date;
        }

        public (string CompanyName, string Address) GetCompanyInfo()
        {
            var company = _dbContext.CompanyInfos.FirstOrDefault();

            if (company == null)
                return (string.Empty, string.Empty);

            return (company.Name, company.DhakaOfficeAddress);
        }

        #region Lookup List

        public async Task<IList<TransactionHistory>> GetAllTransactionHistory(DateTime? startDate, DateTime? endDate)
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

                return await _dbContext.TransactionHistories
                    .Where(x => !x.IsDeleted && x.Date.Date >= fromDate && x.Date.Date <= toDate)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<TransactionHistory>();
            }
        }

        public async Task<IList<Lov>> GetAllProductList()
        {
            try
            {
                return await _dbContext.Products
                    .Include(x => x.ProductsSize)
                .Where(x => !x.Supplier.IsDisable)
                .Select(x => new Lov
                {
                    Id = x.Id,
                    Name = x.Name + " - " + x.ProductsSize.Name
                })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllSupplierList()
        {
            try
            {
                return await _dbContext.Suppliers
                .Where(x => !x.IsDisable)
                .Select(x => new Lov
                {
                    Id = x.Id,
                    Name = x.Name,
                })               
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllSizeList()
        {
            try
            {
                return await _dbContext.ProductsSize
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                         
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllRoadList()
        {
            try
            {
                return await _dbContext.Road
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                         
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllCustomerTypeList()
        {
            try
            {
                return await _dbContext.CustomerType
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                        
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }



        public async Task<IList<Lovd>> GetAllCustomerList()
        {
            try
            {
                return await _dbContext.Customers
                     .Select(x => new Lovd
                     {
                         Id = x.Id,
                         Name = x.Name,

                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lovd>();
            }
        }

        public async Task<IList<Lov>> GetAllEmployeeTypeList()
        {
            try
            {
                return await _dbContext.EmployeeType
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                        
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }



        public async Task<IList<Lovd>> GetAllEmployeeList()
        {
            try
            {

                var result = await _dbContext.Employees
                  .Where(x => !x.IsDisable)
                  .Select(x => new Lovd
                  {
                      Id = x.Id,
                      Name = x.Name + " (" + x.EmployeeType.Name + ")",

                      Desc = x.Phone
                  })
                 .OrderBy(x => x.Name)
                 .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<IList<Lov>> GetAllExpenseTypeList()
        {
            try
            {
                return await _dbContext.ExpenseType
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                         
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllDailyExpenseTypeList()
        {
            try
            {
                return await _dbContext.DailyExpenseType
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                         
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllReasonofAdjustmentList()
        {
            try
            {
                return await _dbContext.ReasonofAdjustment
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                         
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllPaymentMethodList()
        {
            try
            {
                return await _dbContext.PaymentMethod
                    .Where(x => x.Id < 10 || x.Id > 15 && !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllShippingMethodList()
        {
            try
            {
                return await _dbContext.ShippingMethod
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name,
                         
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public async Task<IList<Lov>> GetAllShopList()
        {
            try
            {
                return await _dbContext.Shop
                    .Where(x => !x.IsDeleted)
                     .Select(x => new Lov
                     {
                         Id = x.Id,
                         Name = x.Name + " (" + x.Area + ")",
                         
                     })
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        //public async Task<IList<OrderExportToPdfDTO>> OrderExportToPdfList(int orderId)
        //{
        //    try
        //    {
        //        var result = _dbContext.OrderDetails
        //            .Include(x => x.Product)
        //            .ThenInclude(x => x.ProductsSize)
        //            .Where(x => x.OrderId == orderId)
        //             .Select(x => new OrderExportToPdfDTO
        //             {
        //                 ProductName = x.Product.DisplayNameSize,
        //                 // For shipped quantities - breakdown the total quantity
        //                 S_CB = (x.Product.CartunToPiece.HasValue && x.Product.CartunToPiece.Value != 0)
        //                            ? x.Quantity / x.Product.CartunToPiece.Value : 0,
        //                 S_PD = (x.Product.BoxToPiece.HasValue && x.Product.BoxToPiece.Value != 0)
        //                            ? (x.Quantity % x.Product.CartunToPiece.Value) / x.Product.BoxToPiece.Value : 0,
        //                 S_PQ = (x.Product.CartunToPiece.HasValue && x.Product.BoxToPiece.HasValue &&
        //                 x.Product.CartunToPiece.Value != 0 && x.Product.BoxToPiece.Value != 0)
        //            ? (x.Quantity % x.Product.CartunToPiece.Value) % x.Product.BoxToPiece.Value :
        //              (x.Product.BoxToPiece.HasValue && x.Product.BoxToPiece.Value != 0)
        //              ? x.Quantity % x.Product.BoxToPiece.Value : x.Quantity,

        //                 // For return quantities - same breakdown
        //                 R_CB = (x.ReturnQuantity.HasValue && x.Product.CartunToPiece.HasValue && x.Product.CartunToPiece.Value != 0)
        //                 ? x.ReturnQuantity.Value / x.Product.CartunToPiece.Value : 0,
        //                 R_PD = (x.ReturnQuantity.HasValue && x.Product.BoxToPiece.HasValue && x.Product.BoxToPiece.Value != 0)
        //                 ? (x.ReturnQuantity.Value % x.Product.CartunToPiece.Value) / x.Product.BoxToPiece.Value : 0,
                        
        //                 R_PQ = (x.ReturnQuantity.HasValue && x.Product.CartunToPiece.HasValue &&
        //                 x.Product.BoxToPiece.HasValue && x.Product.CartunToPiece.Value != 0 &&
        //                 x.Product.BoxToPiece.Value != 0)
        //            ? (x.ReturnQuantity.Value % x.Product.CartunToPiece.Value) % x.Product.BoxToPiece.Value :
        //              (x.ReturnQuantity.HasValue && x.Product.BoxToPiece.HasValue &&
        //               x.Product.BoxToPiece.Value != 0)
        //              ? x.ReturnQuantity.Value % x.Product.BoxToPiece.Value : x.ReturnQuantity ?? 0
        //             })

        //        .ToList();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<OrderExportToPdfDTO>();
        //    }
        //}

        #endregion Lookup List

        #region Size
        public bool UpdateSize(ProductsSize Size)
        {
            return AddUpdateEntity(Size);
        }

        public int CreateSize(ProductsSize Size)
        {
            var lastId = _dbContext.ProductsSize
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            Size.Id = lastId + 1;
            _dbContext.ProductsSize.Add(Size);
            _dbContext.SaveChanges();

            return Size.Id;
        }

        public ProductsSize GetSize(int id)
        {
            try
            {
                return _dbContext.ProductsSize.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<ProductsSize>> GetAllSize()
        {
            try
            {
                return await _dbContext.ProductsSize
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<ProductsSize>();
            }
        }

        public bool DeleteSize(int id)
        {
            try
            {
                var Size = _dbContext.ProductsSize.FirstOrDefault(c => c.Id == id);

                if (Size is null)
                {
                    return false;
                }


                var isDelete = _dbContext.Products.Any(c => c.ProductsSizeId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(Size);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    Size.IsDeleted = true;
                    _dbContext.Update(Size);
                    _dbContext.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion Size

        #region ReasonofAdjustment
        public bool UpdateReasonofAdjustment(ReasonofAdjustment ReasonofAdjustment)
        {
            return AddUpdateEntity(ReasonofAdjustment);
        }

        public int CreateReasonofAdjustment(ReasonofAdjustment ReasonofAdjustment)
        {
            var lastId = _dbContext.ReasonofAdjustment
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            ReasonofAdjustment.Id = lastId + 1;
            _dbContext.ReasonofAdjustment.Add(ReasonofAdjustment);
            _dbContext.SaveChanges();

            return ReasonofAdjustment.Id;
        }

        public ReasonofAdjustment GetReasonofAdjustment(int id)
        {
            try
            {
                return _dbContext.ReasonofAdjustment.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<ReasonofAdjustment>> GetAllReasonofAdjustment()
        {
            try
            {
                return await _dbContext.ReasonofAdjustment
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<ReasonofAdjustment>();
            }
        }

        public bool DeleteReasonofAdjustment(int id)
        {
            try
            {
                var ReasonofAdjustment = _dbContext.ReasonofAdjustment.FirstOrDefault(c => c.Id == id);

                if (ReasonofAdjustment is null)
                {
                    return false;
                }


                var isDelete = _dbContext.ProductConsumptions.Any(c => c.ReasonofAdjustmentId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(ReasonofAdjustment);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    ReasonofAdjustment.IsDeleted = true;
                    _dbContext.Update(ReasonofAdjustment);
                    _dbContext.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion ReasonofAdjustment

        #region Road
        public bool UpdateRoad(Road Road)
        {
            return AddUpdateEntity(Road);
        }

        public int CreateRoad(Road Road)
        {
            var lastId = _dbContext.Road
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            Road.Id = lastId + 1;
            _dbContext.Road.Add(Road);
            _dbContext.SaveChanges();

            return Road.Id;
        }

        public Road GetRoad(int id)
        {
            try
            {
                return _dbContext.Road.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<Road>> GetAllRoad()
        {
            try
            {
                return await _dbContext.Road
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Road>();
            }
        }

        public bool DeleteRoad(int id)
        {
            try
            {
                var Road = _dbContext.Road.FirstOrDefault(c => c.Id == id);

                if (Road is null)
                {
                    return false;
                }

                var isDelete = _dbContext.Orders.Any(c => c.SelectedRoad.Contains(Road.Name));

                if (!isDelete)
                {
                    _dbContext.Remove(Road);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    Road.IsDeleted = true;
                    _dbContext.Update(Road);
                    _dbContext.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion Shop

        #region TransactionHistory
        public bool UpdateTransactionHistory(TransactionHistory TransactionHistory)
        {
            return AddUpdateEntity(TransactionHistory);
        }

        public bool DeleteTransactionHistory(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var enity = _dbContext.TransactionHistories.FirstOrDefault(x => x.Id == id);

                if (enity is not null)
                {
                    enity.IsDeleted = true;
                    _dbContext.Update(enity);
                    _dbContext.SaveChanges();
                }

                if (enity.BalanceIn.HasValue && enity.BalanceIn.Value > 0)
                {
                    BalanceOutTransactionHistories(enity.Id, enity.BalanceIn.Value);
                }
                else if (enity.BalanceOut.HasValue && enity.BalanceOut.Value > 0)
                {
                    BalanceInTransactionHistories(enity.Id, enity.BalanceOut.Value);
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

        public int CreateTransactionHistory(TransactionHistory TransactionHistory)
        {
            AddUpdateEntity(TransactionHistory);
            return TransactionHistory.Id;
        }

        public double GetLatestBalance()
        {
            return _dbContext.TransactionHistories
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Select(x => x.CurrentBalance ?? 0)
                .FirstOrDefault();
        }

        #endregion TransactionHistory

        #region Shop
        public bool UpdateShop(Shop Shop)
        {
            return AddUpdateEntity(Shop);
        }

        public int CreateShop(Shop Shop)
        {
            var lastId = _dbContext.Shop
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            Shop.Id = lastId + 1;
            _dbContext.Shop.Add(Shop);
            _dbContext.SaveChanges();

            return Shop.Id;
        }

        public Shop GetShop(int id)
        {
            try
            {
                return _dbContext.Shop.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<Shop>> GetAllShop()
        {
            try
            {
                return await _dbContext.Shop
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Shop>();
            }
        }

        public bool DeleteShop(int id)
        {
            try
            {
                var Shop = _dbContext.Shop.FirstOrDefault(c => c.Id == id);


                if (Shop is null)
                {
                    return false;
                }

                bool isDelete = _dbContext.DSRShopDues.Any(c => c.ShopId == id)
                   || _dbContext.DSRShopPaymentHistories.Any(c => c.ShopId == id);

                    if (!isDelete)
                    {
                        _dbContext.Remove(Shop);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        // If no customers are associated, proceed with deletion
                        Shop.IsDeleted = true;
                        _dbContext.Update(Shop);
                        _dbContext.SaveChanges();
                    }
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion Shop


        #region CustomerType
        public bool UpdateCustomerType(CustomerType CustomerType)
        {
            return AddUpdateEntity(CustomerType);
        }

        public int CreateCustomerType(CustomerType CustomerType)
        {
            var lastId = _dbContext.CustomerType
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            CustomerType.Id = lastId + 1;
            _dbContext.CustomerType.Add(CustomerType);
            _dbContext.SaveChanges();

            return CustomerType.Id;
        }

        public CustomerType GetCustomerType(int id)
        {
            try
            {
                return _dbContext.CustomerType.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<CustomerType>> GetAllCustomerType()
        {
            try
            {
                return await _dbContext.CustomerType
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<CustomerType>();
            }
        }

        public bool DeleteCustomerType(int id)
        {
            try
            {
                var CustomerType = _dbContext.CustomerType.FirstOrDefault(c => c.Id == id);

                if (CustomerType is null)
                {
                    return false;
                }

                var isDelete = _dbContext.Customers.Any(c => c.CustomerTypeId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(CustomerType);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    CustomerType.IsDeleted = true;
                    _dbContext.Update(CustomerType);
                    _dbContext.SaveChanges();
                }
                                 
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion CustomerType


        #region EmployeeType
        public bool UpdateEmployeeType(EmployeeType EmployeeType)
        {
            return AddUpdateEntity(EmployeeType);
        }

        public int CreateEmployeeType(EmployeeType EmployeeType)
        {
            var lastId = _dbContext.EmployeeType
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            EmployeeType.Id = lastId + 1;
            _dbContext.EmployeeType.Add(EmployeeType);
            _dbContext.SaveChanges();

            return EmployeeType.Id;
        }

        public EmployeeType GetEmployeeType(int id)
        {
            try
            {
                return _dbContext.EmployeeType.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<EmployeeType>> GetAllEmployeeType()
        {
            try
            {
                return await _dbContext.EmployeeType
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<EmployeeType>();
            }
        }

        public bool DeleteEmployeeType(int id)
        {
            try
            {
                var EmployeeType = _dbContext.EmployeeType.FirstOrDefault(c => c.Id == id);

                if (EmployeeType is null)
                {
                    return false;
                }

                var isDelete = _dbContext.Employees.Any(c => c.EmployeeTypeId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(EmployeeType);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no Employees are associated, proceed with deletion
                    EmployeeType.IsDeleted = true;
                    _dbContext.Update(EmployeeType);
                    _dbContext.SaveChanges();
                }
                                 
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion EmployeeType


        #region ExpenseType
        public bool UpdateExpenseType(ExpenseType ExpenseType)
        {
            return AddUpdateEntity(ExpenseType);
        }

        public int CreateExpenseType(ExpenseType ExpenseType)
        {
            var lastId = _dbContext.ExpenseType
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            ExpenseType.Id = lastId + 1;
            _dbContext.ExpenseType.Add(ExpenseType);
            _dbContext.SaveChanges();

            return ExpenseType.Id;
        }

        public ExpenseType GetExpenseType(int id)
        {
            try
            {
                return _dbContext.ExpenseType.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<ExpenseType>> GetAllExpenseType()
        {
            try
            {
                return await _dbContext.ExpenseType
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<ExpenseType>();
            }
        }

        public bool DeleteExpenseType(int id)
        {
            try
            {
                var ExpenseType = _dbContext.ExpenseType.FirstOrDefault(c => c.Id == id);

                if (ExpenseType is null)
                {
                    return false;
                }

                var isDelete = _dbContext.Expenses.Any(c => c.ExpenseTypeId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(ExpenseType);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    ExpenseType.IsDeleted = true;
                    _dbContext.Update(ExpenseType);
                    _dbContext.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion ExpenseType

        #region DailyExpenseType
        public bool UpdateDailyExpenseType(DailyExpenseType DailyExpenseType)
        {
            return AddUpdateEntity(DailyExpenseType);
        }

        public int CreateDailyExpenseType(DailyExpenseType DailyExpenseType)
        {
            var lastId = _dbContext.DailyExpenseType
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            DailyExpenseType.Id = lastId + 1;
            _dbContext.DailyExpenseType.Add(DailyExpenseType);
            _dbContext.SaveChanges();

            return DailyExpenseType.Id;
        }

        public DailyExpenseType GetDailyExpenseType(int id)
        {
            try
            {
                return _dbContext.DailyExpenseType.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<DailyExpenseType>> GetAllDailyExpenseType()
        {
            try
            {
                return await _dbContext.DailyExpenseType
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<DailyExpenseType>();
            }
        }

        public bool DeleteDailyExpenseType(int id)
        {
            try
            {
                var DailyExpenseType = _dbContext.DailyExpenseType.FirstOrDefault(c => c.Id == id);

                if (DailyExpenseType is null)
                {
                    return false;
                }


                var isDelete = _dbContext.DailyExpenses.Any(c => c.DailyExpenseTypeId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(DailyExpenseType);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    DailyExpenseType.IsDeleted = true;
                    _dbContext.Update(DailyExpenseType);
                    _dbContext.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion DailyExpenseType

        #region PaymentMethod
        public bool UpdatePaymentMethod(PaymentMethod PaymentMethod)
        {
            return AddUpdateEntity(PaymentMethod);
        }

        public int CreatePaymentMethod(PaymentMethod PaymentMethod)
        {
            var lastId = _dbContext.PaymentMethod
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            PaymentMethod.Id = lastId + 1;
            _dbContext.PaymentMethod.Add(PaymentMethod);
            _dbContext.SaveChanges();

            return PaymentMethod.Id;
        }

        public PaymentMethod GetPaymentMethod(int id)
        {
            try
            {
                return _dbContext.PaymentMethod.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<PaymentMethod>> GetAllPaymentMethod()
        {
            try
            {
                return await _dbContext.PaymentMethod
                    .Where(x => x.Id < 10 || x.Id > 15)
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<PaymentMethod>();
            }
        }

        public bool DeletePaymentMethod(int id)
        {
            try
            {
                var PaymentMethod = _dbContext.PaymentMethod.FirstOrDefault(c => c.Id == id);

                if (PaymentMethod is null)
                {
                    return false;
                }

                bool isDelete = _dbContext.Purchases.Any(c => c.PaymentMethodId == id)
                   || _dbContext.Orders.Any(c => c.PaymentMethodId == id)
                   || _dbContext.Expenses.Any(c => c.PaymentMethodId == id)
                   || _dbContext.SupplierPaymentHistories.Any(c => c.PaymentMethodId == id)
                   || _dbContext.CustomerPaymentHistories.Any(c => c.PaymentMethodId == id)
                   || _dbContext.DSRShopPaymentHistories.Any(c => c.PaymentMethodId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(PaymentMethod);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    PaymentMethod.IsDeleted = true;
                    _dbContext.Update(PaymentMethod);
                    _dbContext.SaveChanges();
                }

                _dbContext.Remove(PaymentMethod);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion PaymentMethod

        #region ShippingMethod
        public bool UpdateShippingMethod(ShippingMethod ShippingMethod)
        {
            return AddUpdateEntity(ShippingMethod);
        }

        public int CreateShippingMethod(ShippingMethod ShippingMethod)
        {
            var lastId = _dbContext.ShippingMethod
                 .AsNoTracking()
                 .OrderByDescending(p => p.Id)
                 .Select(p => p.Id)
                 .FirstOrDefault();

            ShippingMethod.Id = lastId + 1;
            _dbContext.ShippingMethod.Add(ShippingMethod);
            _dbContext.SaveChanges();

            return ShippingMethod.Id;
        }

        public ShippingMethod GetShippingMethod(int id)
        {
            try
            {
                return _dbContext.ShippingMethod.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<ShippingMethod>> GetAllShippingMethod()
        {
            try
            {
                return await _dbContext.ShippingMethod
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<ShippingMethod>();
            }
        }

        public bool DeleteShippingMethod(int id)
        {
            try
            {
                var ShippingMethod = _dbContext.ShippingMethod.FirstOrDefault(c => c.Id == id);

                if (ShippingMethod is null)
                {
                    return false;
                }

                var isDelete = _dbContext.Purchases.Any(c => c.ShippingMethodId == id);

                if (!isDelete)
                {
                    _dbContext.Remove(ShippingMethod);
                    _dbContext.SaveChanges();
                }
                else
                {
                    // If no customers are associated, proceed with deletion
                    ShippingMethod.IsDeleted = true;
                    _dbContext.Update(ShippingMethod);
                    _dbContext.SaveChanges();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<IList<SalesReturnDTO>> GetAllSalesReturn(DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        public Task<IList<SalesHistoryDTO>> GetAllProductSalesHistory(DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        public Task<IList<DSRSalesReturnDTO>> GetDSRAllSalesReturn(DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        public Task<IList<OrderExportToPdfDTO>> OrderExportToPdfList(int orderId)
        {
            throw new NotImplementedException();
        }

        #endregion ShippingMethod

    }
}
