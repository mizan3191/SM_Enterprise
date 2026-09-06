namespace DEALER.DataAccess
{
    public class CustomerManager : BaseDataManager, ICustomer
    {
        public CustomerManager(DEALERContext model) : base(model)
        {
        }


        public int AddCustomerPayment(CustomerPaymentHistory customerPayment)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var lastPayment = _dbContext.CustomerPaymentHistories
                                 .Where(p => p.CustomerId == customerPayment.CustomerId && !p.IsDeleted)
                                 .OrderByDescending(p => p.Id)
                                 .FirstOrDefault();

                double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0; // If no previous payment, due is 0
                double totalDueAfter = totalDueBefore - customerPayment.AmountPaid;

                customerPayment.TotalDueBeforePayment = totalDueBefore;
                customerPayment.TotalDueAfterPayment = totalDueAfter;
                customerPayment.PaymentDate = customerPayment.PaymentDate;

                AddUpdateEntity(customerPayment);

                if (customerPayment.AmountPaid > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                           .AsNoTracking()
                                           .Where(x => !x.IsDeleted)
                                           .OrderByDescending(x => x.Id)
                                           .FirstOrDefault()?.CurrentBalance ?? 0;


                    TransactionHistory transactionHistory = new()
                    {
                        BalanceIn = customerPayment.AmountPaid,
                        BalanceOut = 0,
                        CurrentBalance = existCurrentBalance + customerPayment.AmountPaid,
                        Date = customerPayment.PaymentDate,
                        CustomerPaymentHistoryId = customerPayment.Id,
                        Resone = customerPayment.Customer?.Name != null ? $"Customer Due Payment By {customerPayment.Customer.Name}." : $"Customer Due Payment",
                    };

                    _dbContext.Add(transactionHistory);
                    _dbContext.SaveChanges();
                }


                transaction.Commit();
                return customerPayment.Id;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return 0;
            }
        }


        public bool UpdateCustomerPayment(CustomerPaymentHistory customerPayment)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingTransaction = _dbContext.TransactionHistories
                   .FirstOrDefault(x => x.CustomerPaymentHistoryId == customerPayment.Id && !x.IsDeleted);

                if (existingTransaction == null)
                    throw new Exception("Transaction history not found.");

                var previousAmountPaid = existingTransaction.BalanceIn ?? 0;
                var currentBalanceBeforeUpdate = existingTransaction.CurrentBalance ?? 0;


                var existingPaymentDue = _dbContext.CustomerPaymentHistories
                                    .AsNoTracking()
                                    .FirstOrDefault(p => p.Id == customerPayment.Id)
                                    .TotalDueBeforePayment;

                double totalDueBefore = existingPaymentDue;
                double totalDueAfter = totalDueBefore - customerPayment.AmountPaid;

                customerPayment.TotalDueBeforePayment = totalDueBefore;
                customerPayment.TotalDueAfterPayment = totalDueAfter;
                customerPayment.PaymentDate = customerPayment.PaymentDate;

                //AddUpdateEntity(customerPayment);
                _dbContext.Update(customerPayment);
                _dbContext.SaveChanges();

                var UpDownAmount = existingPaymentDue - customerPayment.TotalDueAfterPayment;


                // Calculate new balance
                double newAmountPaid = customerPayment.AmountPaid;
                double difference = newAmountPaid - previousAmountPaid;

                existingTransaction.BalanceIn = newAmountPaid;
                existingTransaction.CurrentBalance = currentBalanceBeforeUpdate + difference;
                existingTransaction.Date = customerPayment.PaymentDate;

                _dbContext.Update(existingTransaction);
                _dbContext.SaveChanges();

                // Optional: update related transaction history records
                BalanceInTransactionHistories(existingTransaction.Id, difference);


                RecalculateCustomerPaymentsAsync(customerPayment.CustomerId.Value, customerPayment.Id, UpDownAmount);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void RecalculateCustomerPaymentsAsync(int customerId, int id, double amount)
        {
            if (amount == 0)
                return;

            try
            {
                var payments = _dbContext.CustomerPaymentHistories
                                   .Where(p => p.CustomerId == customerId && p.Id > id)
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


        public CustomerPaymentHistory GetCustomerPaymentHistory(int id)
        {
            try
            {
                return _dbContext.CustomerPaymentHistories.SingleOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool DeleteCustomerPayment(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var entity = _dbContext.CustomerPaymentHistories
                    .FirstOrDefault(c => c.Id == id);

                entity.IsDeleted = true;

                _dbContext.Update(entity);
                _dbContext.SaveChanges();

                RecalculateCustomerPaymentsAsync(entity.CustomerId.Value, entity.Id, entity.AmountPaid);


                var existingTransaction = _dbContext.TransactionHistories
                    .FirstOrDefault(x => x.CustomerPaymentHistoryId == entity.Id);

                if (existingTransaction == null)
                    throw new Exception("Transaction history not found.");

                existingTransaction.IsDeleted = true;

                _dbContext.Update(existingTransaction);
                _dbContext.SaveChanges();

                BalanceOutTransactionHistories(existingTransaction.Id, entity.AmountPaid);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

        }


        #region Customer

        public bool UpdateCustomer(Customer Customer)
        {
            try
            {
                _dbContext.Update(Customer);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public int CreateCustomer(Customer Customer)
        {
            AddUpdateEntity(Customer);
            return Customer.Id;
        }

        public Customer GetCustomer(int id)
        {
            try
            {
                return _dbContext.Customers.SingleOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetCustomerName(int id)
        {
            try
            {
                return _dbContext.Customers.SingleOrDefault(c => c.Id == id).Name;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<Customer>> GetAllCustomer()
        {
            try
            {
                return await _dbContext.Customers
                .Include(c => c.CustomerType)
                .OrderBy(c => c.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Customer>();
            }
        }

        public async Task<IList<Lov>> GetAllClients()
        {
            try
            {
                return await _dbContext.Customers.
                Select(x => new Lov
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Lov>();
            }
        }

        public bool DeleteCustomer(int id)
        {
            return RemoveEntity<Customer>(id);
        }

        #endregion Customer

        #region Employee
        public bool UpdateEmployee(Employee employee)
        {
            try
            {
                _dbContext.Update(employee);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public int CreateEmployee(Employee employee)
        {
            AddUpdateEntity(employee);
            return employee.Id;
        }

        public Employee GetEmployee(int id)
        {
            try
            {
                return _dbContext.Employees.SingleOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetEmployeeName(int id)
        {
            try
            {
                return _dbContext.Employees.SingleOrDefault(c => c.Id == id).Name;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<Employee>> GetAllEmployees()
        {
            try
            {
                return await _dbContext.Employees
                .Include(c => c.EmployeeType)
                .OrderBy(c => c.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Employee>();
            }
        }

        public bool DeleteEmployee(int id)
        {
            return RemoveEntity<Employee>(id);
        }

#endregion Employee
    }
}