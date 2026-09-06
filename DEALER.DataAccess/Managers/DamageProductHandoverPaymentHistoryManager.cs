
//namespace DEALER.DataAccess
//{
//    public class DamageProductHandoverPaymentHistoryManager : BaseDataManager, IDamageProductHandoverPaymentHistory
//    {
//        public DamageProductHandoverPaymentHistoryManager(DEALERContext model) : base(model)
//        {
//        }

//        public int CreateDamageProductHandoverPaymentHistory(DamageProductHandoverPaymentHistory DamageProductHandoverPaymentHistory)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();
//            try
//            {
//                AddUpdateEntity(DamageProductHandoverPaymentHistory);


//                if (DamageProductHandoverPaymentHistory.AmountPaid > 0)
//                {
//                    var existCurrentBalance = _dbContext.TransactionHistories
//                                           .AsNoTracking()
//                                           .Where(x => !x.IsDeleted)
//                                           .OrderByDescending(x => x.Id)
//                                           .FirstOrDefault()?.CurrentBalance ?? 0;


//                    TransactionHistory transactionHistory = new()
//                    {
//                        BalanceIn = DamageProductHandoverPaymentHistory.AmountPaid,
//                        BalanceOut = 0,
//                        CurrentBalance = existCurrentBalance + DamageProductHandoverPaymentHistory.AmountPaid,
//                        Date = DamageProductHandoverPaymentHistory.Date,
//                        DamageProductHandoverPaymentHistoryId = DamageProductHandoverPaymentHistory.Id,
//                        Resone = "Damage Product Due Payment",
//                    };

//                    _dbContext.Add(transactionHistory);
//                    _dbContext.SaveChanges();
//                }
//                transaction.Commit();
//                return DamageProductHandoverPaymentHistory.Id;
//            }
//            catch (Exception ex)
//            {
//                transaction.Rollback();
//                return 0;
//            }
//        }

//        public bool UpdateDamageProductHandoverPaymentHistory(DamageProductHandoverPaymentHistory DamageProductHandoverPaymentHistory)
//        {
            

//            using var transaction = _dbContext.Database.BeginTransaction();
//            try
//            {
//                _dbContext.Update(DamageProductHandoverPaymentHistory);
//                _dbContext.SaveChanges();

//                var existingTransaction = _dbContext.TransactionHistories
//                     .FirstOrDefault(x => x.DamageProductHandoverPaymentHistoryId == DamageProductHandoverPaymentHistory.Id);

//                if (existingTransaction == null)
//                    throw new Exception("Transaction history not found.");

//                var previousAmountPaid = existingTransaction.BalanceIn ?? 0;
//                var currentBalanceBeforeUpdate = existingTransaction.CurrentBalance ?? 0;

//                // Calculate new balance
//                double newAmountPaid = DamageProductHandoverPaymentHistory.AmountPaid;
//                double difference = newAmountPaid - previousAmountPaid;

//                existingTransaction.BalanceIn = newAmountPaid;
//                existingTransaction.CurrentBalance = currentBalanceBeforeUpdate + difference;
//                existingTransaction.Date = DamageProductHandoverPaymentHistory.Date;

//                _dbContext.Update(existingTransaction);
//                _dbContext.SaveChanges();

//                // Optional: update related transaction history records
//                BalanceInTransactionHistories(existingTransaction.Id, difference);


//                transaction.Commit();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                transaction.Rollback();
//                return false;
//            }

//        }

//        public bool DeleteDamageProductHandoverPaymentHistory(int id)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();
//            try
//            {
//                var entity = _dbContext.DamageProductHandoverPaymentHistories
//                     .FirstOrDefault(x => x.Id == id);

//                entity.IsDeleted = true;
//                _dbContext.Update(entity);
//                _dbContext.SaveChanges();

//                var transactionHistory = _dbContext.TransactionHistories
//                     .FirstOrDefault(x => x.DamageProductHandoverPaymentHistoryId == entity.Id);

//                transactionHistory.IsDeleted = true;
//                _dbContext.Update(transactionHistory);
//                _dbContext.SaveChanges();

//                BalanceInTransactionHistories(transactionHistory.Id, entity.AmountPaid);

//                transaction.Commit();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                transaction.Rollback();
//                return false;
//            }
//        }

//        public async Task<IList<DamageProductHandoverPaymentHistory>> GetAllDamageProductHandoverPaymentHistory(int supplierId)
//        {
//            return await _dbContext.DamageProductHandoverPaymentHistories
//                .Include(x => x.Supplier)
//                .Include(x => x.Customer)
//                .Where(x => x.SupplierId == supplierId && !x.IsDeleted)
//                .OrderByDescending(d => d.Id)
//                .ToArrayAsync();

//        }

//        public DamageProductHandoverPaymentHistory GetDamageProductHandoverPaymentHistory(int id)
//        {
//            try
//            {
//                return _dbContext.DamageProductHandoverPaymentHistories.FirstOrDefault(x => x.Id == id);
//            }
//            catch (Exception e)
//            {
//                throw;
//            }
//        }
//    }
//}
