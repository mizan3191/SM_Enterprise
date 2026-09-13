namespace DEALER.DataAccess
{
    public class StaffSalaryManager : BaseDataManager, IStaffSalary
    {
        public StaffSalaryManager(DEALERContext model) : base(model)
        {
        }

        // =====================================================
        // CREATE
        // =====================================================
        public int CreateStaffSalary(StaffSalary staffSalary)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                AddUpdateEntity(staffSalary);

                if (staffSalary.Amount > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                               .AsNoTracking()
                                               .Where(x => !x.IsDeleted)
                                               .OrderByDescending(x => x.Id)
                                               .FirstOrDefault()?.CurrentBalance ?? 0;

                    TransactionHistory transactionHistory = new TransactionHistory()
                    {
                        BalanceIn = 0,
                        BalanceOut = staffSalary.Amount,
                        CurrentBalance = existCurrentBalance - staffSalary.Amount,
                        Date = staffSalary.SalaryDate,
                        StaffSalaryId = staffSalary.Id,
                        Resone = "Staff Salary",
                        IsDeleted = false
                    };

                    _dbContext.Add(transactionHistory);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return staffSalary.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // =====================================================
        // UPDATE
        // =====================================================
        public bool UpdateStaffSalary(StaffSalary staffSalary)
        {
            var previousPaymentHistory = _dbContext.TransactionHistories
                .AsNoTracking()
                .FirstOrDefault(x => x.StaffSalaryId == staffSalary.Id && !x.IsDeleted)?.BalanceOut;

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                _dbContext.Update(staffSalary);
                _dbContext.SaveChanges();

                var updatedPayment = (previousPaymentHistory ?? 0) - staffSalary.Amount;

                var existTransactionHistory = _dbContext.TransactionHistories
                                            .Where(x => x.StaffSalaryId == staffSalary.Id && !x.IsDeleted)
                                            .OrderByDescending(x => x.Id)
                                            .FirstOrDefault();

                if (existTransactionHistory != null)
                {
                    existTransactionHistory.BalanceOut = staffSalary.Amount;
                    existTransactionHistory.Date = staffSalary.SalaryDate;
                    existTransactionHistory.CurrentBalance =
                        (existTransactionHistory.CurrentBalance + (previousPaymentHistory ?? 0)) - staffSalary.Amount;

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

        // =====================================================
        // GET BY ID
        // =====================================================
        public StaffSalary GetStaffSalary(int id)
        {
            try
            {
                return _dbContext.StaffSalaries
                    .Include(x => x.Employee)
                    .Include(x => x.PaymentMethod)
                    .Include(x => x.PaymentType)
                    .SingleOrDefault(c => c.Id == id && !c.IsDeleted);
            }
            catch
            {
                return null;
            }
        }

        // =====================================================
        // GET ALL (with date filter)
        // =====================================================
        public async Task<IList<StaffSalary>> GetAllStaffSalaries(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DateTime fromDate = startDate.HasValue
                    ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
                    : DateTime.Today.AddDays(-30);

                DateTime toDate = endDate.HasValue
                    ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
                    : DateTime.Today.AddDays(1).AddSeconds(-1);

                return await _dbContext.StaffSalaries
                    .Include(x => x.Employee)
                    .Include(x => x.PaymentMethod)
                    .Include(x => x.PaymentType)
                    .Where(x => !x.IsDeleted
                                && x.SalaryDate.Date >= fromDate
                                && x.SalaryDate.Date <= toDate)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch
            {
                return new List<StaffSalary>();
            }
        }

        // =====================================================
        // DELETE (soft delete + reverse TransactionHistory)
        // =====================================================
        public bool DeleteStaffSalary(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var staffSalary = _dbContext.StaffSalaries
                    .FirstOrDefault(c => c.Id == id);

                if (staffSalary == null) return false;

                staffSalary.IsDeleted = true;
                _dbContext.Update(staffSalary);
                _dbContext.SaveChanges();

                var existTransactionHistory = _dbContext.TransactionHistories
                                            .FirstOrDefault(x => x.StaffSalaryId == staffSalary.Id && !x.IsDeleted);

                if (existTransactionHistory != null)
                {
                    existTransactionHistory.IsDeleted = true;
                    _dbContext.Update(existTransactionHistory);
                    _dbContext.SaveChanges();

                    BalanceInTransactionHistories(existTransactionHistory.Id, staffSalary.Amount);
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