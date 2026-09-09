namespace DEALER.DataAccess
{
    public class ExpenseManager : BaseDataManager, IExpense
    {
        public ExpenseManager(DEALERContext model) : base(model)
        {
        }

        public bool UpdateExpense(Expense expense)
        {
            var previousPaymentHistory = _dbContext.TransactionHistories.AsNoTracking().FirstOrDefault(x => x.ExpenseId == expense.Id).BalanceOut;

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                _dbContext.Update(expense);
                _dbContext.SaveChanges();

                var updatedPayment = previousPaymentHistory - expense.Amount;

                var existTransactionHistory = _dbContext.TransactionHistories
                                            .Where(x => x.ExpenseId == expense.Id)
                                           .OrderByDescending(x => x.Id)
                                           .FirstOrDefault();


                existTransactionHistory.BalanceOut = expense.Amount;
                existTransactionHistory.Date = expense.ExpenseDate;
                existTransactionHistory.CurrentBalance = (existTransactionHistory.CurrentBalance + previousPaymentHistory) - expense.Amount;

                _dbContext.Update(existTransactionHistory);
                _dbContext.SaveChanges();

                BalanceInTransactionHistories(existTransactionHistory.Id, updatedPayment.Value);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int CreateExpense(Expense expense)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                AddUpdateEntity(expense);

                if (expense.Amount > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                               .AsNoTracking()
                                               .OrderByDescending(x => x.Id)
                                               .FirstOrDefault()?.CurrentBalance ?? 0;


                    TransactionHistory transactionHistory = new TransactionHistory()
                    {
                        BalanceIn = 0,
                        BalanceOut = expense.Amount,
                        CurrentBalance = existCurrentBalance - expense.Amount,
                        Date = expense.ExpenseDate,
                        ExpenseId = expense.Id,
                        Resone = "Expense",
                    };

                    _dbContext.Add(transactionHistory);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return expense.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Expense GetExpense(int id)
        {
            try
            {
                return _dbContext.Expenses.SingleOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<Expense>> GetAllExpenses(DateTime? startDate, DateTime? endDate)
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


                return await _dbContext.Expenses
                    .Include(x => x.ExpenseType)
                    .Include(x => x.PaymentMethod)
                    .Include(x => x.Employee)
                    .Where(x => !x.IsDeleted && x.ExpenseDate.Date >= fromDate && x.ExpenseDate.Date <= toDate)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Expense>();
            }
        }

        public async Task<IList<Expense>> GetAllStaffSalary(DateTime? startDate, DateTime? endDate)
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


                return await _dbContext.Expenses
                    .Include(x => x.ExpenseType)
                    .Include(x => x.PaymentMethod)
                    .Include(x => x.Employee)
                    .Where(x => !x.IsDeleted && x.ExpenseTypeId == 4 && x.ExpenseDate.Date >= fromDate && x.ExpenseDate.Date <= toDate)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Expense>();
            }
        }

        public bool DeleteExpense(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var expense = _dbContext.Expenses
                    .FirstOrDefault(c => c.Id == id);

                expense.IsDeleted = true;
                _dbContext.Update(expense);
                _dbContext.SaveChanges();

                var existTransactionHistory = _dbContext.TransactionHistories
                                            .FirstOrDefault(x => x.ExpenseId == expense.Id);

                existTransactionHistory.IsDeleted = true;

                _dbContext.Update(existTransactionHistory);
                _dbContext.SaveChanges();

                BalanceInTransactionHistories(existTransactionHistory.Id, expense.Amount);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}