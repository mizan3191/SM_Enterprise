namespace DEALER.DataAccess
{
    public class SupplierManager : BaseDataManager, ISupplier
    {
        public SupplierManager(DEALERContext model) : base(model)
        {
        }

        public bool UpdateSupplier(Supplier Supplier)
        {
            return AddUpdateEntity(Supplier);
        }

        public int CreateSupplier(Supplier Supplier)
        {
            AddUpdateEntity(Supplier);
            return Supplier.Id;
        }

        public bool DeleteSupplier(int id)
        {
            return RemoveEntity<Supplier>(id);
        }

        public Supplier GetSupplier(int id)
        {
            return _dbContext.Suppliers.FirstOrDefault(c => c.Id == id);
        }

        public async Task<IList<Supplier>> GetAllSupplier()
        {
            return await _dbContext.Suppliers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseDTO>> GetAllPurchases(DateTime? startDate, DateTime? endDate)
        {
            // Default to last 3 days if no date is provided
            DateTime fromDate = startDate.HasValue
                ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
                : DateTime.Today.AddDays(-30); // default to last 7 days

            DateTime toDate = endDate.HasValue
                ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
                : DateTime.Today.AddDays(1).AddSeconds(-1); // today till 23:59:59


            var purchases = await _dbContext.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseDetails)
                .Include(p => p.ShippingMethod)
                .Where(p => !p.IsDeleted && p.Date.Date >= fromDate && p.Date.Date <= toDate)
                .Select(p => new PurchaseDTO
                {
                    Id = p.Id,
                    Name = p.Supplier.Name,
                    SupplierId = p.Supplier.Id,
                    Comments = p.Comments,
                    ShippingMethod = p.ShippingMethod.Name,
                    OrderDate = p.Date,
                    DamageProductDueAdjustment = p.DamageProductDueAdjustment,
                    TotalPrice = p.TotalAmount,
                })
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return purchases;
        }

        public async Task<IList<SupplierPaymentHistoryDTO>> GetSupplierPaymentHistoryById(int supplierId)
        {
            var supplierPaymentHistory = await _dbContext.SupplierPaymentHistories
                  .Where(x => x.SupplierId == supplierId && !x.IsDeleted)
                  .Select(x => new SupplierPaymentHistoryDTO
                  {
                      Id = x.Id,
                      TotalAmountThisPurchase = x.TotalAmountThisPurchase,
                      AmountPaid = x.AmountPaid,
                      TotalDueBeforePayment = x.TotalDueBeforePayment,
                      TotalDueAfterPayment = x.TotalDueAfterPayment,
                      PaymentDate = x.PaymentDate,
                      PaymentMethod = x.PaymentMethod.Name,
                      TransactionID = x.TransactionID,
                      Comments = x.Comments,
                      Number = x.Number,
                      IsDisabled = x.PurchaseId.HasValue
                  })
                  .OrderByDescending(x => x.Id)
                  .ToListAsync();

            return supplierPaymentHistory;
        }

        public int AddSupplierPayment(SupplierPaymentHistory supplierPayment)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var lastPayment = _dbContext.SupplierPaymentHistories
                                 .Where(p => p.SupplierId == supplierPayment.SupplierId && !p.IsDeleted)
                                 .OrderByDescending(p => p.Id)
                                 .FirstOrDefault();

                double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0; // If no previous payment, due is 0
                double totalDueAfter = totalDueBefore - supplierPayment.AmountPaid;

                supplierPayment.TotalDueBeforePayment = totalDueBefore;
                supplierPayment.TotalDueAfterPayment = totalDueAfter;
                supplierPayment.PaymentDate = supplierPayment.PaymentDate;

                AddUpdateEntity(supplierPayment);


                if (supplierPayment.AmountPaid > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                           .Where(x => !x.IsDeleted)
                                           .AsNoTracking()
                                           .OrderByDescending(x => x.Id)
                                           .FirstOrDefault()?.CurrentBalance ?? 0;


                    TransactionHistory transactionHistory = new()
                    {
                        BalanceIn = 0,
                        BalanceOut = supplierPayment.AmountPaid,
                        CurrentBalance = existCurrentBalance - supplierPayment.AmountPaid,
                        Date = supplierPayment.PaymentDate,
                        SupplierPaymentHistoryId = supplierPayment.Id,
                        Resone = supplierPayment.Supplier?.Name != null ? $"Paid the supplier duo to {supplierPayment.Supplier.Name}." : $"Supplier Due Payment",
                    };

                    _dbContext.Add(transactionHistory);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return supplierPayment.Id;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return 0;
            }
        }

        public bool UpdateSupplierPayment(SupplierPaymentHistory supplierPayment)
        {
            var previousPaymentHistory = _dbContext.TransactionHistories
                            .AsNoTracking()
                            .FirstOrDefault(x => x.SupplierPaymentHistoryId == supplierPayment.Id && !x.IsDeleted)
                            .BalanceOut;

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var secondLastPayment = _dbContext.SupplierPaymentHistories
                                .Where(p => p.SupplierId == supplierPayment.SupplierId && !p.IsDeleted)
                                .OrderByDescending(p => p.Id)
                                .Skip(1)
                                .FirstOrDefault();

                double totalDueBefore = secondLastPayment?.TotalDueAfterPayment ?? 0; // If no previous payment, due is 0
                double totalDueAfter = totalDueBefore - supplierPayment.AmountPaid;

                supplierPayment.TotalDueBeforePayment = totalDueBefore;
                supplierPayment.TotalDueAfterPayment = totalDueAfter;
                supplierPayment.PaymentDate = supplierPayment.PaymentDate;


                _dbContext.Update(supplierPayment);
                _dbContext.SaveChanges();

                //return AddUpdateEntity(supplierPayment);

                var updatedPayment = previousPaymentHistory - supplierPayment.AmountPaid;

                var existTransactionHistory = _dbContext.TransactionHistories
                                            .Where(x => x.SupplierPaymentHistoryId == supplierPayment.Id && !x.IsDeleted)
                                           .OrderByDescending(x => x.Id)
                                           .FirstOrDefault();

                existTransactionHistory.BalanceOut = supplierPayment.AmountPaid;
                existTransactionHistory.CurrentBalance = (existTransactionHistory.CurrentBalance + previousPaymentHistory) - supplierPayment.AmountPaid;

                _dbContext.Update(existTransactionHistory);
                _dbContext.SaveChanges();

                BalanceOutTransactionHistories(existTransactionHistory.Id, updatedPayment.Value);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool DeleteSupplierPayment(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // Get the payment to be deleted
                var SupplierPaymentHistory = _dbContext.SupplierPaymentHistories
                    .FirstOrDefault(p => p.Id == id);

                if (SupplierPaymentHistory is null)
                {
                    return false;
                }

                // Adjust the next payment's totals by adding back the deleted payment amount

                SupplierPaymentHistory.IsDeleted = true;
                SupplierPaymentHistory.PaymentDate = DateTime.Now;
                _dbContext.Update(SupplierPaymentHistory);


                var existTransactionHistory = _dbContext.TransactionHistories
                                            .Where(x => x.SupplierPaymentHistoryId == SupplierPaymentHistory.Id && !x.IsDeleted)
                                           .OrderByDescending(x => x.Id)
                                           .FirstOrDefault();

                existTransactionHistory.IsDeleted = true;
                existTransactionHistory.CurrentBalance = existTransactionHistory.CurrentBalance + SupplierPaymentHistory.AmountPaid;

                _dbContext.Update(existTransactionHistory);
                _dbContext.SaveChanges();

                BalanceInTransactionHistories(existTransactionHistory.Id, SupplierPaymentHistory.AmountPaid);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public SupplierPaymentHistory GetSupplierPaymentHistory(int id)
        {
            try
            {
                return _dbContext.SupplierPaymentHistories.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        //public bool DeleteSupplierPayment(int id)
        //{
        //    try
        //    {
        //        var supplier = _dbContext.SupplierPaymentHistories.FirstOrDefault(c => c.Id == id);

        //        if (supplier is null)
        //        {
        //            return false;
        //        }


        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        public IEnumerable<SupplierDueDTO> GetSupplierDueHistory()
        {
            try
            {
                var query = from supplier in _dbContext.Suppliers
                            join payment in _dbContext.SupplierPaymentHistories.Where(p => !p.IsDeleted)
                                 on supplier.Id equals payment.SupplierId into paymentsGroup
                            select new SupplierDueDTO
                            {
                                Id = supplier.Id,
                                Name = supplier.Name,
                                Phone = supplier.Phone,
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
                return Enumerable.Empty<SupplierDueDTO>();
            }
        }

        public IEnumerable<OrdersByPersonDTO> GetAllPurchasesByPerson(int personId)
        {
            try
            {
                var orders = _dbContext.Purchases
                    .Include(p => p.PurchaseDetails)
                        .ThenInclude(pd => pd.Product)
                            .ThenInclude(p => p.ProductsSize)
                    .Where(p => p.SupplierId == personId && !p.IsDeleted)
                    .Select(p => new OrdersByPersonDTO
                    {
                        OrderId = p.Id,
                        ProductsName = string.Join(", ", p.PurchaseDetails.Select(pd => pd.Product.DisplayNameSize)), // Assuming DisplayNameSize includes size info
                        OrderDate = p.Date,
                    })
                    .OrderByDescending(p => p.OrderId)
                    .ToList();

                return orders;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<OrdersByPersonDTO>();
            }
        }

    }
}