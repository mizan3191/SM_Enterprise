namespace DEALER.DataAccess
{
    public class DailyExpenseManager : BaseDataManager, IDailyExpense
    {
        public DailyExpenseManager(DEALERContext model) : base(model)
        {
        }

        public bool DeleteDailyExpense(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var dailyExpenseEntity = _dbContext.DailyExpenses
                    .FirstOrDefault(x => x.Id == id);

                if (dailyExpenseEntity is null)
                {
                    return false;
                }

                dailyExpenseEntity.IsDeleted = true;

                _dbContext.Update(dailyExpenseEntity);
                _dbContext.SaveChanges();

                if (dailyExpenseEntity.OrderId > 0)
                {
                    var customerPaymentHistories = _dbContext.CustomerPaymentHistories
                                    .FirstOrDefault(p => p.EmployeeId == dailyExpenseEntity.EmployeeId
                                    && p.OrderId == dailyExpenseEntity.OrderId
                                    && p.DailyExpenseId == dailyExpenseEntity.Id);

                    customerPaymentHistories.IsDeleted = true;

                    _dbContext.Update(customerPaymentHistories);
                    _dbContext.SaveChanges();

                    var amount = customerPaymentHistories.AmountPaid;

                    RecalculateCustomerPaymentsAsync(customerPaymentHistories.EmployeeId.Value, customerPaymentHistories.Id, amount);

                }

                var existTransactionHistory = _dbContext.TransactionHistories
                                           .FirstOrDefault(x => x.DailyExpenseId == id);


                existTransactionHistory.IsDeleted = true;
                _dbContext.Update(existTransactionHistory);
                _dbContext.SaveChanges();

                BalanceInTransactionHistories(existTransactionHistory.Id, dailyExpenseEntity.Amount);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        public int CreateDailyExpenseInOrder(DailyExpense DailyExpense)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                AddUpdateEntity(DailyExpense);

                if (DailyExpense.OrderId > 0)
                {
                    var lastPayment = _dbContext.CustomerPaymentHistories
                                    .Where(p => p.EmployeeId == DailyExpense.EmployeeId)
                                    .OrderByDescending(p => p.Id)
                                    .FirstOrDefault();

                    double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0; // If no previous payment, due is 0
                    double totalDueAfter = totalDueBefore - DailyExpense.Amount;

                    // Add new entry to Customer Payment History
                    var payment = new CustomerPaymentHistory()
                    {
                        EmployeeId = DailyExpense.EmployeeId,
                        OrderId = DailyExpense.OrderId,
                        DailyExpenseId = DailyExpense.Id,
                        PaymentDate = DailyExpense.Date,
                        PaymentMethodId = 1,
                        TransactionID = string.Empty,
                        Number = string.Empty,
                        TotalAmountThisOrder = 0,
                        AmountPaid = DailyExpense.Amount,
                        TotalDueBeforePayment = totalDueBefore,
                        TotalDueAfterPayment = totalDueAfter
                    };

                    _dbContext.CustomerPaymentHistories.Add(payment);
                    _dbContext.SaveChanges();
                }


                if (DailyExpense.Amount > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                               .AsNoTracking()
                                               .OrderByDescending(x => x.Id)
                                               .FirstOrDefault()?.CurrentBalance ?? 0;


                    TransactionHistory transactionHistory = new()
                    {
                        BalanceIn = 0,
                        BalanceOut = DailyExpense.Amount,
                        CurrentBalance = existCurrentBalance - DailyExpense.Amount,
                        Date = DailyExpense.Date,
                        DailyExpenseId = DailyExpense.Id,
                        Resone = DailyExpense.DailyExpenseType?.Name != null ? DailyExpense.DailyExpenseType.Name : $"Daily Expense",
                    };

                    _dbContext.Add(transactionHistory);
                }

                _dbContext.SaveChanges();
                transaction.Commit();

                return DailyExpense.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int CreateDailyExpense(DailyExpense DailyExpense)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                AddUpdateEntity(DailyExpense);

                if (DailyExpense.Amount > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                               .AsNoTracking()
                                               .OrderByDescending(x => x.Id)
                                               .FirstOrDefault()?.CurrentBalance ?? 0;

                    TransactionHistory transactionHistory = new()
                    {
                        BalanceIn = 0,
                        BalanceOut = DailyExpense.Amount,
                        CurrentBalance = existCurrentBalance - DailyExpense.Amount,
                        Date = DailyExpense.Date,
                        DailyExpenseId = DailyExpense.Id,
                        Resone = DailyExpense.DailyExpenseType?.Name != null ? DailyExpense.DailyExpenseType.Name : $"Daily Expense",
                    };

                    _dbContext.Add(transactionHistory);
                }

                _dbContext.SaveChanges();
                transaction.Commit();

                return DailyExpense.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateDailyExpenseInOrder(DailyExpense DailyExpense)
        {
            var previousExpense = _dbContext.DailyExpenses
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == DailyExpense.Id);

            var previousTransaction = _dbContext.TransactionHistories
                .AsNoTracking()
                .FirstOrDefault(x => x.DailyExpenseId == DailyExpense.Id);

            if (previousExpense == null || previousTransaction == null)
                return false;

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // Step 1: Update DailyExpense
                _dbContext.Update(DailyExpense);
                _dbContext.SaveChanges();

                // Step 2: Get existing customer payment record
                var payment = _dbContext.CustomerPaymentHistories
                    .FirstOrDefault(p => p.DailyExpenseId == DailyExpense.Id && p.EmployeeId == DailyExpense.EmployeeId);

                double upDownAmount = 0;

                if (payment != null)
                {
                    upDownAmount = payment.AmountPaid - DailyExpense.Amount;

                    // First revert old payment
                    double oldDue = payment.TotalDueAfterPayment + payment.AmountPaid;

                    // Then apply new payment
                    double newDue = oldDue - DailyExpense.Amount;

                    payment.PaymentDate = DailyExpense.Date;
                    payment.AmountPaid = DailyExpense.Amount;
                    payment.TotalDueAfterPayment = newDue;

                    _dbContext.Update(payment);
                    _dbContext.SaveChanges();
                }


                var existingTransaction = _dbContext.TransactionHistories
                    .Where(x => x.DailyExpenseId == DailyExpense.Id)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();

                if (existingTransaction != null)
                {
                    // Revert old balance and apply new
                    double updatedPayment = previousTransaction.BalanceOut.Value - DailyExpense.Amount;

                    existingTransaction.BalanceOut = DailyExpense.Amount;
                    existingTransaction.CurrentBalance = (existingTransaction.CurrentBalance + previousTransaction.BalanceOut) - DailyExpense.Amount;

                    _dbContext.Update(existingTransaction);
                    _dbContext.SaveChanges();

                    // Update subsequent transactions
                    BalanceInTransactionHistories(existingTransaction.Id, updatedPayment);
                }


                // Step 4: Recalculate downstream customer payments
                if (payment != null)
                {
                    RecalculateCustomerPaymentsAsync(payment.EmployeeId.Value, payment.Id, upDownAmount);
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



        public bool UpdateDailyExpense(DailyExpense DailyExpense)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingTransaction = _dbContext.TransactionHistories
                    .FirstOrDefault(x => x.DailyExpenseId == DailyExpense.Id);

                if (existingTransaction == null)
                    throw new Exception("Transaction history not found.");

                var previousAmount = existingTransaction.BalanceOut ?? 0;
                var currentBalanceBeforeUpdate = existingTransaction.CurrentBalance ?? 0;

                _dbContext.Update(DailyExpense);
                _dbContext.SaveChanges();

                double newAmount = DailyExpense.Amount;
                double difference = previousAmount - newAmount; // Expense reduces balance

                existingTransaction.BalanceOut = newAmount;
                existingTransaction.CurrentBalance = currentBalanceBeforeUpdate + difference;
                existingTransaction.Date = DailyExpense.Date;

                _dbContext.Update(existingTransaction);
                _dbContext.SaveChanges();

                BalanceInTransactionHistories(existingTransaction.Id, difference);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        private void RecalculateCustomerPaymentsAsync(int employeeId, int id, double amount)
        {
            if (amount == 0)
                return;

            try
            {
                var payments = _dbContext.CustomerPaymentHistories
                                   .Where(p => p.EmployeeId == employeeId && p.Id > id)
                                   .OrderBy(p => p.Id)
                                   .ToList();

                if (!payments.Any() || payments.Count() == 0 || payments is null)
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


        public DailyExpense GetDailyExpense(int id)
        {
            try
            {
                return _dbContext.DailyExpenses
                    .FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<DailyExpense>> GetAllDailyExpense(DateTime? startDate, DateTime? endDate)
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


                return await _dbContext.DailyExpenses
                    .Include(x => x.Employee)
                    .Include(x => x.DailyExpenseType)
                    .Where(x => !x.IsDeleted && x.Date.Date >= fromDate && x.Date.Date <= toDate)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<DailyExpense>();
            }
        }

        public IList<DailyExpense> GetAllDailyExpense(int orderId)
        {
            try
            {
                return _dbContext.DailyExpenses
                    .Include(x => x.Employee)
                    .Include(x => x.DailyExpenseType)
                    .Include(x => x.Order)
                    .Where(x => x.OrderId == orderId && !x.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<DailyExpense>();
            }
        }
        public async Task<IList<DailyExpenseDTO>> GetAllDailyExpenseByOrderId(int orderId)
        {
            try
            {
                return await _dbContext.DailyExpenses
                    .Include(x => x.Employee)
                    .Include(x => x.DailyExpenseType)
                    .Include(x => x.Order)
                    .Where(x => x.OrderId == orderId && !x.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new DailyExpenseDTO
                    {
                        Id = x.Id,
                        CustomerName = x.Employee.Name,
                        OrderId = x.OrderId,
                        DailyExpenseType = x.DailyExpenseType.Name,
                        Description = x.Description,
                        Amount = x.Amount,
                        Date = x.Date
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<DailyExpenseDTO>();
            }
        }

    }
}