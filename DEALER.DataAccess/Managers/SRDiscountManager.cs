//namespace DEALER.DataAccess
//{
//    public class SRDiscountManager : BaseDataManager, ISRDiscount
//    {
//        public SRDiscountManager(DEALERContext model) : base(model)
//        {
//        }


//        public bool DeleteSRDiscount(int id)
//        {
//            try
//            {
//                using var transaction = _dbContext.Database.BeginTransaction();

//                var SRDiscount = _dbContext.SRDiscounts.FirstOrDefault(c => c.Id == id);

//                if (SRDiscount == null)
//                    return false;

//                // Update main return entity
//                SRDiscount.Date = DateTime.UtcNow;
//                SRDiscount.IsDeleted = true;

//                _dbContext.Update(SRDiscount);
//                _dbContext.SaveChanges();

//                var entity = _dbContext.CustomerPaymentHistories
//                                 .FirstOrDefault(p => p.CustomerId == SRDiscount.DSRCustomerId
//                                 && p.OrderId == SRDiscount.OrderId
//                                 && p.SRDiscountId == id);


//                entity.IsDeleted = true;
//                _dbContext.Update(entity);
//                _dbContext.SaveChanges();


//                RecalculateCustomerPaymentsAsync(entity.CustomerId, entity.Id, entity.AmountPaid);
//                transaction.Commit();

//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }


//        public bool UpdateSRDiscount(SRDiscount SRDiscount)
//        {
//            var previousAmount = _dbContext.SRDiscounts
//                .AsNoTracking()
//                .FirstOrDefault(x => x.Id == SRDiscount.Id).DiscountAmount;

//            using var transaction = _dbContext.Database.BeginTransaction();
//            _dbContext.Update(SRDiscount);
//            _dbContext.SaveChanges();
//            //AddUpdateEntity(SRDiscount);


//            var payment = _dbContext.CustomerPaymentHistories
//                             .FirstOrDefault(p => p.OrderId == SRDiscount.OrderId
//                             && p.SRDiscountId == SRDiscount.Id
//                             && p.CustomerId == SRDiscount.DSRCustomerId);


//            var UpDownAmount = payment.AmountPaid - SRDiscount.DiscountAmount;

//            if (payment != null)
//            {
//                // double totalDueBefore = payment.TotalDueAfterPayment;
//                double totalDueAfter = (payment.TotalDueAfterPayment + previousAmount) - SRDiscount.DiscountAmount;

//                payment.PaymentDate = SRDiscount.Date;
//                payment.AmountPaid = SRDiscount.DiscountAmount;
//                //payment.TotalDueBeforePayment = totalDueBefore;
//                payment.TotalDueAfterPayment = totalDueAfter;
//            }

//            _dbContext.Update(payment);
//            _dbContext.SaveChanges();

//            //AddUpdateEntity(payment);
//            _dbContext.SaveChanges();
//            transaction.Commit();

//            RecalculateCustomerPaymentsAsync(payment.CustomerId, payment.Id, UpDownAmount);
//            return true;
//        }

//        private void RecalculateCustomerPaymentsAsync(int customerId, int id, double amount)
//        {
//            if (amount == 0)
//                return;

//            try
//            {
//                var payments = _dbContext.CustomerPaymentHistories
//                                   .Where(p => p.CustomerId == customerId && p.Id > id)
//                                   .OrderBy(p => p.Id)
//                                   .ToList();

//                if (!payments.Any() || payments.Count() == 0 || payments is null)
//                {
//                    return;
//                }

//                foreach (var payment in payments)
//                {
//                    payment.TotalDueBeforePayment += amount;
//                    payment.TotalDueAfterPayment += amount;

//                    _dbContext.Update(payment);
//                    _dbContext.SaveChanges();
//                }

                
//            }
//            catch
//            {
//                throw;
//            }
//        }


//        public int CreateSRDiscount(SRDiscount SRDiscount)
//        {
//            using var transaction = _dbContext.Database.BeginTransaction();
//            AddUpdateEntity(SRDiscount);


//            var lastPayment = _dbContext.CustomerPaymentHistories
//                             .Where(p => p.CustomerId == SRDiscount.DSRCustomerId
//                             ).OrderByDescending(p => p.Id)
//                             .FirstOrDefault();

//            double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0; // If no previous payment, due is 0
//            double totalDueAfter = totalDueBefore - SRDiscount.DiscountAmount;

//            // Add new entry to Customer Payment History
//            var payment = new CustomerPaymentHistory()
//            {
//                CustomerId = SRDiscount.DSRCustomerId,
//                OrderId = SRDiscount.OrderId,
//                SRDiscountId = SRDiscount.Id,
//                PaymentDate = SRDiscount.Date,
//                PaymentMethodId = 13,
//                TransactionID = string.Empty,
//                Number = string.Empty,
//                TotalAmountThisOrder = 0,
//                AmountPaid = SRDiscount.DiscountAmount,
//                TotalDueBeforePayment = totalDueBefore,
//                TotalDueAfterPayment = totalDueAfter
//            };

//            AddUpdateEntity(payment);

//            _dbContext.SaveChanges();
//            transaction.Commit();
//            return SRDiscount.Id;
//        }

//        public SRDiscount GetSRDiscountById(int id)
//        {
//            try
//            {
//                return _dbContext.SRDiscounts.FirstOrDefault(c => c.Id == id);
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }

//        public SRDiscount GetSRDiscountByOrderId(int orderId)
//        {
//            try
//            {
//                return _dbContext.SRDiscounts.FirstOrDefault(c => c.OrderId == orderId);
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }

//        public SRDiscount GetSRDiscount(int customerId, int orderId)
//        {
//            try
//            {
//                return _dbContext.SRDiscounts
//                    .FirstOrDefault(c => c.DSRCustomerId == customerId && c.OrderId == orderId);
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }

//        public async Task<IList<SRDiscount>> GetAllSRDiscount(int customerId)
//        {
//            try
//            {
//                return await _dbContext.SRDiscounts
//                    .Include(x => x.DSRCustomer)
//                    .Where(x => x.CustomerId == customerId)
//                    .OrderByDescending(x => x.Id)
//                    .ToListAsync();
//            }
//            catch (Exception ex)
//            {
//                return new List<SRDiscount>();
//            }
//        }

//        public IList<SRDiscountDTO> GetAllSRDiscount_ByOrderId(int orderId)
//        {
//            try
//            {
//                return _dbContext.SRDiscounts
//                    .Include(x => x.Customer)
//                    .Include(x => x.DSRCustomer)
//                    .Where(x => x.OrderId == orderId && !x.IsDeleted)
//                    .OrderByDescending(x => x.Id)
//                    .Select(x => new SRDiscountDTO
//                    {
//                        Id = x.Id,
//                        OrderId = x.OrderId,
//                        SRName = x.Customer.Name,
//                        DSRName = x.DSRCustomer.Name,
//                        DiscountAmount = x.DiscountAmount,
//                        Date = x.Date
//                    })
//                    .ToList();
//            }
//            catch (Exception ex)
//            {
//                // Log the exception if needed
//                return new List<SRDiscountDTO>();
//            }
//        }

//        public async Task<IList<SRDiscount>> GetAllSRDiscountByOrderId(int orderId)
//        {
//            try
//            {
//                return await _dbContext.SRDiscounts
//                    .Include(x => x.Customer)
//                    .Include(x => x.DSRCustomer)
//                    .Where(x => x.OrderId == orderId && !x.IsDeleted)
//                    .OrderByDescending(x => x.Id)
//                    .ToListAsync();
//            }
//            catch (Exception ex)
//            {
//                return new List<SRDiscount>();
//            }
//        }

//        public async Task<IList<SRDiscount>> GetAllSRDiscountList(int customerId, DateTime? startDate, DateTime? endDate)
//        {
//            try
//            {
//                // Default to last 3 days if no date is provided
//                DateTime fromDate = startDate.HasValue
//                    ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
//                    : DateTime.Today.AddDays(-30); // default to last 7 days

//                DateTime toDate = endDate.HasValue
//                    ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
//                    : DateTime.Today.AddDays(1).AddSeconds(-1); // today till 23:59:59


//                return await _dbContext.SRDiscounts
//                    .Include(x => x.Customer)
//                    .Include(x => x.Order)
//                    .Include(x => x.DSRCustomer)
//                    .Where(x => x.Date.Date >= fromDate && x.Date.Date <= toDate && !x.IsDeleted && x.CustomerId == customerId)
//                    .OrderByDescending(x => x.Id)
//                    .ToListAsync();
//            }
//            catch (Exception ex)
//            {
//                return new List<SRDiscount>();
//            }
//        }

//        public async Task<IList<SRDiscountSummary>> GetTotalDiscountPerSRAsync()
//        {
//            try
//            {
//                var discounts = await _dbContext.SRDiscounts
//                    .Where(x => !x.IsDeleted)
//                    .GroupBy(x => new { x.CustomerId, x.Customer.Name })
//                    .Select(g => new
//                    {
//                        g.Key.CustomerId,
//                        g.Key.Name,
//                        TotalDiscount = g.Sum(x => x.DiscountAmount)
//                    })
//                    .ToListAsync();

//                var payments = await _dbContext.SRPaymentHistories
//                    .Where(x => !x.IsDeleted)
//                    .GroupBy(x => new { x.CustomerId, x.Customer.Name })
//                    .Select(g => new
//                    {
//                        g.Key.CustomerId,
//                        TotalPaid = g.Sum(x => x.AmountPaid)
//                    })
//                    .ToListAsync();

//                var result = discounts.Select(d => new SRDiscountSummary
//                {
//                    CustomerId = d.CustomerId,
//                    CustomerName = d.Name,
//                    TotalDueAmount = d.TotalDiscount - (payments.FirstOrDefault(p => p.CustomerId == d.CustomerId)?.TotalPaid ?? 0)
//                }).ToList();

//                return result;
//            }
//            catch (Exception ex)
//            {
//                return new List<SRDiscountSummary>();
//            }
//        }

//        public async Task<IList<SRDiscountDuePaymentSummary>> GetSRDiscountDuePaymentSummary(int customerId)
//        {
//            try
//            {
//                // Get discount (due) entries
//                var dues = await _dbContext.SRDiscounts
//                    .Where(x => !x.IsDeleted && x.CustomerId == customerId)
//                    .Select(x => new
//                    {
//                        x.CustomerId,
//                        SRName = x.DSRCustomer.Name,
//                        Date = x.Date.Date,
//                        DueAmount = x.DiscountAmount
//                    })
//                    .ToListAsync();

//                // Get payment entries
//                var payments = await _dbContext.SRPaymentHistories
//                    .Where(x => x.CustomerId == customerId && !x.IsDeleted)
//                    .Select(x => new
//                    {
//                        x.CustomerId,
//                        SRName = x.Customer.Name,
//                        Date = x.PaymentDate.Date,
//                        PaidAmount = x.AmountPaid
//                    })
//                    .ToListAsync();

//                // Group and combine data
//                var allData = dues
//                    .GroupBy(x => x.Date)
//                    .Select(g => new SRDiscountTempShopSummary
//                    {
//                        Date = g.Key,
//                        SRDiscountId = g.First().CustomerId,
//                        SRName = g.First().SRName,
//                        DueAmount = g.Sum(x => x.DueAmount),
//                        PaidAmount = 0
//                    })
//                    .ToList();

//                foreach (var paymentGroup in payments.GroupBy(x => x.Date))
//                {
//                    var existing = allData.FirstOrDefault(x => x.Date == paymentGroup.Key);
//                    if (existing != null)
//                    {
//                        existing.PaidAmount = paymentGroup.Sum(x => x.PaidAmount);
//                    }
//                    else
//                    {
//                        allData.Add(new SRDiscountTempShopSummary
//                        {
//                            Date = paymentGroup.Key,
//                            SRDiscountId = paymentGroup.First().CustomerId,
//                            SRName = paymentGroup.First().SRName,
//                            DueAmount = 0,
//                            PaidAmount = paymentGroup.Sum(x => x.PaidAmount)
//                        });
//                    }
//                }

//                // Project to final result
//                var result = allData
//                    .OrderByDescending(x => x.Date)
//                    .Select(x => new SRDiscountDuePaymentSummary
//                    {
//                        SRDiscountId = x.SRDiscountId,
//                        SRName = x.SRName,
//                        Date = x.Date,
//                        DueAmount = x.DueAmount,
//                        PaidAmount = x.PaidAmount
//                    })
//                    .ToList();

//                return result;
//            }
//            catch (Exception)
//            {
//                return new List<SRDiscountDuePaymentSummary>();
//            }
//        }

//    }
//}
