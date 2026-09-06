//namespace DEALER.DataAccess
//{
//    public class DamageProductReturnsManager : BaseDataManager, IDamageProductReturns
//    {
//        public DamageProductReturnsManager(DEALERContext model) : base(model)
//        {
//        }

//        public bool CreateDamageProductReturn(DamageProductReturn DamageProductReturn)
//        {
//            if (DamageProductReturn == null || DamageProductReturn.DamageProductReturnDetails == null || !DamageProductReturn.DamageProductReturnDetails.Any())
//            {
//                return false;
//            }

//            try
//            {
//                var details = DamageProductReturn.DamageProductReturnDetails;
//                DamageProductReturn.DamageProductReturnDetails = null;

//                _dbContext.DamageProductReturns.Add(DamageProductReturn);

//                _dbContext.SaveChanges(); // Save to generate Customer Product Return ID

//                var returnId = DamageProductReturn.Id;
//                foreach (var item in details)
//                {
//                    if (item != null)
//                    {
//                        DamageProductReturnDetails DamageProductReturnDetails = new();

//                        DamageProductReturnDetails = item;
//                        DamageProductReturnDetails.Id = 0;
//                        DamageProductReturnDetails.DamageProductReturnId = returnId;
//                        _dbContext.DamageProductReturnDetails.Add(item);

//                        DamageProduct damageProduct = new()
//                        {
//                            CustomerId = DamageProductReturn.CustomerId,
//                            OrderId = DamageProductReturn.OrderId,
//                            UnitPrice = DamageProductReturn.DamageProductReturnDetails.FirstOrDefault(x => x.ProductId == item.ProductId).UnitPrice,
//                            ProductId = item.ProductId,
//                            Quantity = item.Quantity,
//                            Date= DamageProductReturn.Date,
//                        };

//                        _dbContext.DamageProducts.Add(damageProduct);
//                        _dbContext.SaveChanges();
//                    }
//                }

//                var lastPayment = _dbContext.CustomerPaymentHistories
//                                 .Where(p => p.CustomerId == DamageProductReturn.CustomerId)
//                                 .OrderByDescending(p => p.Id)
//                                 .FirstOrDefault();

//                double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0; // If no previous payment, due is 0
//                double totalDueAfter = totalDueBefore - DamageProductReturn.TotalAmount;

//                // Add new entry to Customer Payment History
//                var payment = new CustomerPaymentHistory()
//                {
//                    CustomerId = DamageProductReturn.CustomerId,
//                    OrderId = DamageProductReturn.OrderId,
//                    DamageProductReturnId = DamageProductReturn.Id,
//                    PaymentDate = DamageProductReturn.Date,
//                    PaymentMethodId = 12,
//                    TransactionID = string.Empty,
//                    Number = string.Empty,
//                    TotalAmountThisOrder = 0,
//                    AmountPaid = DamageProductReturn.TotalAmount,
//                    TotalDueBeforePayment = totalDueBefore,
//                    TotalDueAfterPayment = totalDueAfter
//                };

//                _dbContext.CustomerPaymentHistories.Add(payment);
//                _dbContext.SaveChanges();

//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        public bool UpdateDamageProductReturn(DamageProductReturn DamageProductReturn)
//        {
//            if (DamageProductReturn == null)
//            {
//                return false;
//            }

//            try
//            {
//                var previousAmount = _dbContext.DamageProductReturns
//                    .AsNoTracking()
//                    .FirstOrDefault(x => x.Id == DamageProductReturn.Id)
//                    .TotalAmount;

//                using var transaction = _dbContext.Database.BeginTransaction();

//                var removeAllEntity = _dbContext.DamageProducts.Where(x => x.OrderId == DamageProductReturn.OrderId).ToList();

//                _dbContext.RemoveRange(removeAllEntity);

//                foreach (var item in DamageProductReturn.DamageProductReturnDetails)
//                {
//                    if (item != null)
//                    {
//                        DamageProduct damageProduct = new()
//                        {
//                            CustomerId = DamageProductReturn.CustomerId,
//                            OrderId = DamageProductReturn.OrderId,
//                            UnitPrice = DamageProductReturn.DamageProductReturnDetails.FirstOrDefault(x => x.ProductId == item.ProductId).UnitPrice,
//                            ProductId = item.ProductId,
//                            Quantity = item.Quantity,
//                            Date = DamageProductReturn.Date,
//                        };

//                        _dbContext.DamageProducts.Add(damageProduct);
//                        _dbContext.SaveChanges();
//                    }
//                }

//                AddUpdateEntity(DamageProductReturn);

//                // Update payment history for this return
//                var payment = _dbContext.CustomerPaymentHistories
//                    .FirstOrDefault(p => p.DamageProductReturnId == DamageProductReturn.Id);

//                var UpDownAmount = payment.AmountPaid - DamageProductReturn.TotalAmount;

//                if (payment != null)
//                {
//                    double totalDueBefore = payment.TotalDueBeforePayment;
//                    double totalDueAfter = (payment.TotalDueAfterPayment + previousAmount) - DamageProductReturn.TotalAmount;

//                    payment.PaymentDate = DamageProductReturn.Date;
//                    payment.AmountPaid = DamageProductReturn.TotalAmount;
//                    payment.TotalDueAfterPayment = totalDueAfter;
//                }

//                _dbContext.SaveChanges();
//                transaction.Commit();

//                RecalculateCustomerPaymentsAsync(payment.CustomerId, payment.Id, UpDownAmount);

//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
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
//                }

//                _dbContext.SaveChanges();
//            }
//            catch
//            {
//                throw;
//            }
//        }



//        public bool DeleteDamageProductReturn(int DamageProductReturnId)
//        {
//            try
//            {
//                using var transaction = _dbContext.Database.BeginTransaction();
                
//                var DamageProductReturn = _dbContext.DamageProductReturns
//                    .Include(x => x.DamageProductReturnDetails)
//                    .FirstOrDefault(x => x.Id == DamageProductReturnId);

//                if (DamageProductReturn == null)
//                    return false;

//                // Update main return entity
//                DamageProductReturn.Date = DateTime.UtcNow;
//                DamageProductReturn.IsDeleted = true;

//                var lastPayment = _dbContext.CustomerPaymentHistories
//                                 .Where(p => p.CustomerId == DamageProductReturn.CustomerId)
//                                 .OrderByDescending(p => p.Id)
//                                 .FirstOrDefault();

//                double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0; // If no previous payment, due is 0
//                double totalDueAfter = totalDueBefore + DamageProductReturn.TotalAmount;

//                // Add new entry to Customer Payment History
//                var payment = new CustomerPaymentHistory()
//                {
//                    CustomerId = DamageProductReturn.CustomerId,
//                    OrderId = DamageProductReturn.OrderId,
//                    DamageProductReturnId = DamageProductReturn.Id,
//                    PaymentDate = DateTime.Now,
//                    PaymentMethodId = 12,
//                    TransactionID = string.Empty,
//                    Number = string.Empty,
//                    TotalAmountThisOrder = 0,
//                    AmountPaid = DamageProductReturn.TotalAmount,
//                    TotalDueBeforePayment = totalDueBefore,
//                    TotalDueAfterPayment = totalDueAfter
//                };

//                _dbContext.CustomerPaymentHistories.Add(payment);

//                _dbContext.SaveChanges();
//                transaction.Commit();

//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        public async Task<DamageProductReturn> GetDamageProductReturnByOrderId(int orderId)
//        {
//            try
//            {
//                return await _dbContext.DamageProductReturns
//                    .Include(o => o.DamageProductReturnDetails)
//                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
//            }
//            catch (Exception ex)
//            {
//                // Optionally log the exception
//                return null;
//            }
//        }

//        public async Task<DamageProductReturn> GetDamageProductReturnById(int id)
//        {
//            try
//            {
//                DamageProductReturn DamageProductReturn = await _dbContext.DamageProductReturns
//                    .Include(o => o.DamageProductReturnDetails)
//                    .FirstOrDefaultAsync(o => o.Id == id);

//                if (DamageProductReturn == null)
//                    return null;

//                return DamageProductReturn;
//            }
//            catch (Exception ex)
//            {
//                // Optionally log the exception
//                return null;
//            }
//        }

//        public async Task<IEnumerable<DamageProductReturnDTO>> GetAllCustomerReturnProducts(int orderId)
//        {
//            try
//            {
//                var orders = await _dbContext.DamageProductReturns
//                        .Include(o => o.Customer)
//                        .Include(o => o.DamageProductReturnDetails)
//                        .ThenInclude(d => d.Product)
//                        .Where(o => o.IsDeleted == false && o.OrderId == orderId)
//                        .OrderByDescending(o => o.Id)
//                        .Select(o => new DamageProductReturnDTO
//                        {
//                            Id = o.Id,
//                            OrderId = o.OrderId,
//                            CustomerName = o.Customer.Name,
//                            Products = string.Join(", ",
//                                o.DamageProductReturnDetails.Select(d =>
//                                    d.Product.Name + "(" + d.Quantity + ")")),
//                            OrderDate = o.Date,
//                            TotalPrice = o.TotalAmount
//                        })
//                        .ToListAsync();

//                return orders;
//            }
//            catch (Exception ex)
//            {
//                return Enumerable.Empty<DamageProductReturnDTO>();
//            }
//        }

//    }
//}