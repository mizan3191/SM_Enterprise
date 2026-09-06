//namespace DEALER.DataAccess
//{
//    public class SRPaymentHistoryManager : BaseDataManager, ISRPaymentHistory
//    {
//        public SRPaymentHistoryManager(DEALERContext model) : base(model)
//        {
//        }


//        public bool DeleteSRPaymentHistory(int id)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();

//            try
//            {
//                var entity = _dbContext.SRPaymentHistories
//                    .FirstOrDefault(x => x.Id == id);

//                entity.IsDeleted = true;

//                _dbContext.Update(entity);
//                _dbContext.SaveChanges();

//                var existingTransaction = _dbContext.TransactionHistories
//                    .FirstOrDefault(x => x.SRPaymentHistoryId == entity.Id);

//                if (existingTransaction == null)
//                    throw new Exception("Transaction history not found.");

//                existingTransaction.IsDeleted = true;

//                _dbContext.Update(existingTransaction);
//                _dbContext.SaveChanges();

//                BalanceOutTransactionHistories(existingTransaction.Id, entity.AmountPaid);

//                transaction.Commit();
//                return true;
//            }
//            catch
//            {
//                transaction.Rollback();
//                throw;
//            }
//        }

//        public bool UpdateSRPaymentHistory(SRPaymentHistory SRPaymentHistory)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();

//            try
//            {
//                var existingTransaction = _dbContext.TransactionHistories
//                    .FirstOrDefault(x => x.SRPaymentHistoryId == SRPaymentHistory.Id);

//                if (existingTransaction == null)
//                    throw new Exception("Transaction history not found.");

//                var previousAmountPaid = existingTransaction.BalanceIn ?? 0;
//                var currentBalanceBeforeUpdate = existingTransaction.CurrentBalance ?? 0;

//                // Update main entity
//                _dbContext.Update(SRPaymentHistory);
//                _dbContext.SaveChanges();

//                // Calculate new balance
//                double newAmountPaid = SRPaymentHistory.AmountPaid;
//                double difference = newAmountPaid - previousAmountPaid;

//                existingTransaction.BalanceIn = newAmountPaid;
//                existingTransaction.CurrentBalance = currentBalanceBeforeUpdate + difference;
//                existingTransaction.Date = SRPaymentHistory.PaymentDate;

//                _dbContext.Update(existingTransaction);
//                _dbContext.SaveChanges();

//                BalanceInTransactionHistories(existingTransaction.Id, difference);

//                transaction.Commit();
//                return true;
//            }
//            catch
//            {
//                transaction.Rollback();
//                throw;
//            }
//        }


//        public int CreateSRPaymentHistory(SRPaymentHistory SRPaymentHistory)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();

//            try
//            {
//                AddUpdateEntity(SRPaymentHistory);

//                if (SRPaymentHistory.AmountPaid > 0)
//                {
//                    var existCurrentBalance = _dbContext.TransactionHistories
//                                               .AsNoTracking()
//                                               .OrderByDescending(x => x.Id)
//                                               .FirstOrDefault()?.CurrentBalance ?? 0;


//                    TransactionHistory transactionHistory = new()
//                    {
//                        BalanceIn = SRPaymentHistory.AmountPaid,
//                        BalanceOut = 0,
//                        CurrentBalance = existCurrentBalance + SRPaymentHistory.AmountPaid,
//                        Date = SRPaymentHistory.PaymentDate,
//                        SRPaymentHistoryId = SRPaymentHistory.Id,
//                        Resone = SRPaymentHistory.Customer?.Name != null ? $"SR {SRPaymentHistory.Customer.Name} Paid" : $"SR Payment",
//                    };

//                    _dbContext.Add(transactionHistory);
//                    _dbContext.SaveChanges();
//                }

//                transaction.Commit();

//                return SRPaymentHistory.Id;
//            }
//            catch
//            {
//                transaction.Rollback();
//                throw;
//            }
//        }

//        public SRPaymentHistory GetSRPaymentHistory(int id)
//        {
//            try
//            {
//                return _dbContext.SRPaymentHistories.FirstOrDefault(c => c.Id == id);
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }


//        public async Task<IList<SRPaymentHistory>> GetAllSRPaymentHistory(int customerId)
//        {
//            try
//            {
//                return await _dbContext.SRPaymentHistories
//                    .Include(x => x.Customer)
//                    .Include(x => x.PaymentMethod)
//                    .Where(x => x.CustomerId == customerId && !x.IsDeleted)
//                    .OrderByDescending(x => x.Id)
//                    .ToListAsync();
//            }
//            catch (Exception ex)
//            {
//                return new List<SRPaymentHistory>();
//            }
//        }
//    }
//}