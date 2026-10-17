namespace DEALER.DataAccess
{
    public class YearlyExpenseManager : BaseDataManager, IYearlyExpense
    {
        public YearlyExpenseManager(DEALERContext model) : base(model)
        {
        }

        public int CreateYearlyExpense(YearlyExpense expense)
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
                        Date = expense.Date,
                        YearlyExpenseId = expense.Id,
                        Resone = "Yearly Expense",
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

        public bool UpdateYearlyExpense(YearlyExpense expense)
        {
            var previousPaymentHistory = _dbContext.TransactionHistories
                                            .AsNoTracking()
                                            .Where(x => x.YearlyExpenseId == expense.Id)
                                            .OrderByDescending(x => x.Id)
                                            .FirstOrDefault()?.BalanceOut ?? 0;

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                _dbContext.Update(expense);
                _dbContext.SaveChanges();

                var updatedPayment = previousPaymentHistory - expense.Amount;

                var existTransactionHistory = _dbContext.TransactionHistories
                                            .Where(x => x.YearlyExpenseId == expense.Id)
                                            .OrderByDescending(x => x.Id)
                                            .FirstOrDefault();

                if (existTransactionHistory != null)
                {
                    existTransactionHistory.BalanceOut = expense.Amount;
                    existTransactionHistory.Date = expense.Date;
                    existTransactionHistory.CurrentBalance =
                        (existTransactionHistory.CurrentBalance + previousPaymentHistory) - expense.Amount;

                    _dbContext.Update(existTransactionHistory);
                    _dbContext.SaveChanges();

                    BalanceInTransactionHistories(existTransactionHistory.Id, updatedPayment);
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

        public YearlyExpense GetYearlyExpense(int id)
        {
            try
            {
                return _dbContext.YearlyExpenses.SingleOrDefault(c => c.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IList<YearlyExpense>> GetAllYearlyExpenses(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Date na dile current year (1 Jan - 31 Dec) dekhabe
                DateTime fromDate = startDate.HasValue
                    ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
                    : new DateTime(DateTime.Today.Year, 1, 1, 0, 0, 0);

                DateTime toDate = endDate.HasValue
                    ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
                    : new DateTime(DateTime.Today.Year, 12, 31, 23, 59, 59);

                return await _dbContext.YearlyExpenses
                    .Include(x => x.YearlyExpenseType)
                    .Include(x => x.Employee)
                    .Where(x => !x.IsDeleted && x.Date.Date >= fromDate && x.Date.Date <= toDate)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<YearlyExpense>();
            }
        }

        public bool DeleteYearlyExpense(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var expense = _dbContext.YearlyExpenses.FirstOrDefault(c => c.Id == id);

                if (expense == null)
                    return false;

                expense.IsDeleted = true;
                _dbContext.Update(expense);
                _dbContext.SaveChanges();

                var existTransactionHistory = _dbContext.TransactionHistories
                                            .FirstOrDefault(x => x.YearlyExpenseId == expense.Id);

                if (existTransactionHistory != null)
                {
                    existTransactionHistory.IsDeleted = true;
                    _dbContext.Update(existTransactionHistory);
                    _dbContext.SaveChanges();

                    BalanceInTransactionHistories(existTransactionHistory.Id, expense.Amount);
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
    }
}