namespace DEALER.DataAccess
{
    public class DSRShopPaymentHistoryManager : BaseDataManager, IDSRShopPaymentHistory
    {
        public DSRShopPaymentHistoryManager(DEALERContext model) : base(model)
        {
        }

        public bool UpdateDSRShopPaymentHistory(DSRShopPaymentHistory DSRShopPaymentHistory)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingTransaction = _dbContext.TransactionHistories
                    .FirstOrDefault(x => x.DSRShopPaymentHistoryId == DSRShopPaymentHistory.Id);

                if (existingTransaction == null)
                    throw new Exception("Transaction history not found.");

                var previousAmountPaid = existingTransaction.BalanceIn ?? 0;
                var currentBalanceBeforeUpdate = existingTransaction.CurrentBalance ?? 0;

                // Update main entity
                _dbContext.Update(DSRShopPaymentHistory);
                _dbContext.SaveChanges();

                // Calculate new balance
                double newAmountPaid = DSRShopPaymentHistory.AmountPaid;
                double difference = newAmountPaid - previousAmountPaid;

                existingTransaction.BalanceIn = newAmountPaid;
                existingTransaction.CurrentBalance = currentBalanceBeforeUpdate + difference;
                existingTransaction.Date = DSRShopPaymentHistory.PaymentDate;

                _dbContext.Update(existingTransaction);
                _dbContext.SaveChanges();

                // Optional: update related transaction history records
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
        
        public bool DeleteDSRShopPaymentHistory(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var entity = _dbContext.DSRShopPaymentHistories
                                        .FirstOrDefault(x => x.Id == id);

                entity.IsDeleted = true;
                _dbContext.Update(entity);
                _dbContext.SaveChanges();

                var existingTransaction = _dbContext.TransactionHistories
                    .FirstOrDefault(x => x.DSRShopPaymentHistoryId == id);

                existingTransaction.IsDeleted = true;
                _dbContext.Update(existingTransaction);
                _dbContext.SaveChanges();

                // Optional: update related transaction history records
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

        public int CreateDSRShopPaymentHistory(DSRShopPaymentHistory DSRShopPaymentHistory)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                AddUpdateEntity(DSRShopPaymentHistory);

                if (DSRShopPaymentHistory.AmountPaid > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                               .AsNoTracking()
                                               .OrderByDescending(x => x.Id)
                                               .FirstOrDefault()?.CurrentBalance ?? 0;


                    TransactionHistory transactionHistory = new()
                    {
                        BalanceIn = DSRShopPaymentHistory.AmountPaid,
                        BalanceOut = 0,
                        CurrentBalance = existCurrentBalance + DSRShopPaymentHistory.AmountPaid,
                        Date = DSRShopPaymentHistory.PaymentDate,
                        DSRShopPaymentHistoryId = DSRShopPaymentHistory.Id,
                        Resone = DSRShopPaymentHistory.Shop?.Name != null ? $"{DSRShopPaymentHistory.Shop.Name} Shop Paid " : $"Shop Payment",
                    };

                    _dbContext.Add(transactionHistory);
                }

                _dbContext.SaveChanges();
                transaction.Commit();

                return DSRShopPaymentHistory.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public DSRShopPaymentHistory GetDSRShopPaymentHistory(int id)
        {
            try
            {
                return _dbContext.DSRShopPaymentHistories.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<DSRShopPaymentHistory>> GetAllDSRShopPaymentHistory(int shopId)
        {
            try
            {
                return await _dbContext.DSRShopPaymentHistories
                    .Include(x => x.Customer)
                    .Include(x => x.Shop)
                    .Include(x => x.PaymentMethod)
                    .Where(x => x.ShopId == shopId && !x.IsDeleted && !x.Shop.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<DSRShopPaymentHistory>();
            }
        }


        public async Task<IList<DSRShopPaymentHistory>> GetAllShopPaymentHistories()
        {
            try
            {
                return await _dbContext.DSRShopPaymentHistories
                    .Include(x => x.Customer)
                    .Include(x => x.Shop)
                    .Where(x => !x.Shop.IsDeleted)
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<DSRShopPaymentHistory>();
            }
        }

        public async Task<IList<DSRDueSummary>> GetTotalDuePerShop()
        {
            try
            {
                var discounts = await _dbContext.DSRShopDues
                    .Include(x => x.Shop)
                    .Where(x => !x.IsDeleted && !x.Shop.IsDeleted)
                    .GroupBy(x => new
                    {
                        x.ShopId,
                        x.Shop.Name,
                        x.Shop.ShopOwner,
                        x.Shop.Area,
                        x.Shop.Number
                    })
                    .Select(g => new
                    {
                        g.Key.ShopId,
                        g.Key.Name,
                        g.Key.ShopOwner,
                        g.Key.Area,
                        g.Key.Number,
                        TotalDiscount = g.Sum(x => x.DueAmount)
                    })
                    .ToListAsync();

                var payments = await _dbContext.DSRShopPaymentHistories
                    .Where(x => !x.IsDeleted)
                    .GroupBy(x => new { x.ShopId })
                    .Select(g => new
                    {
                        g.Key.ShopId,
                        TotalPaid = g.Sum(x => x.AmountPaid)
                    })
                    .ToListAsync();

                var result = discounts.Select(d => new DSRDueSummary
                {
                    ShopId = d.ShopId,
                    ShopName = d.Name,
                    OwnerName = d.ShopOwner,
                    Area = d.Area,
                    Number = d.Number,
                    TotalShopDueAmount = d.TotalDiscount - (payments.FirstOrDefault(p => p.ShopId == d.ShopId)?.TotalPaid ?? 0)
                })
                    .OrderBy(x => x.ShopName)
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                return new List<DSRDueSummary>();
            }
        }

        public async Task<IList<ShopDuePaymentSummary>> GetShopDuePaymentSummary(int shopId)
        {
            try
            {
                // Get due entries
                var dues = await _dbContext.DSRShopDues
                    .Where(x => !x.IsDeleted && x.ShopId == shopId)
                    .Select(x => new
                    {
                        x.ShopId,
                        x.Shop.Name,
                        x.Shop.ShopOwner,
                        x.Shop.Area,
                        Date = x.Date.Date,
                        DueAmount = x.DueAmount
                    })
                    .ToListAsync();

                // Get payment entries
                var payments = await _dbContext.DSRShopPaymentHistories
                    .Where(x => x.ShopId == shopId && !x.IsDeleted)
                    .Select(x => new
                    {
                        x.ShopId,
                        x.Shop.Name,
                        x.Shop.ShopOwner,
                        x.Shop.Area,
                        Date = x.PaymentDate.Date,
                        PaidAmount = x.AmountPaid
                    })
                    .ToListAsync();

                // Union dates from both dues and payments
                var allData = dues
                        .GroupBy(x => x.Date)
                        .Select(g => new TempShopSummary
                        {
                            Date = g.Key,
                            ShopId = g.First().ShopId,
                            ShopName = g.First().Name,
                            OwnerName = g.First().ShopOwner,
                            Area = g.First().Area,
                            DueAmount = g.Sum(x => x.DueAmount),
                            PaidAmount = 0
                        })
                        .ToList();

                foreach (var paymentGroup in payments.GroupBy(x => x.Date))
                {
                    var existing = allData.FirstOrDefault(x => x.Date == paymentGroup.Key);
                    if (existing != null)
                    {
                        existing.PaidAmount = paymentGroup.Sum(x => x.PaidAmount);
                    }
                    else
                    {
                        allData.Add(new TempShopSummary
                        {
                            Date = paymentGroup.Key,
                            ShopId = paymentGroup.First().ShopId,
                            ShopName = paymentGroup.First().Name,
                            OwnerName = paymentGroup.First().ShopOwner,
                            Area = paymentGroup.First().Area,
                            DueAmount = 0,
                            PaidAmount = paymentGroup.Sum(x => x.PaidAmount)
                        });
                    }
                }

                // Map to your final model
                var result = allData
                    .OrderByDescending(x => x.Date)
                    .Select(x => new ShopDuePaymentSummary
                    {
                        ShopId = x.ShopId,
                        ShopName = x.ShopName,
                        OwnerName = x.OwnerName,
                        Area = x.Area,
                        Date = x.Date,
                        ShopDueAmount = x.DueAmount,
                        ShopPaidAmount = x.PaidAmount
                    })
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                return new List<ShopDuePaymentSummary>();
            }
        }

        public async Task<IList<ShopDuePaymentListSummary>> GetAllShopDuePaymentListSummary()
        {
            try
            {
                // Get due entries with customer name
                var dues = await _dbContext.DSRShopDues
                    .Include(x => x.Shop)
                    .Where(x => !x.IsDeleted && !x.Shop.IsDeleted)
                    .Include(x => x.Employee)
                    .Select(x => new 
                    {
                        x.ShopId,
                        x.OrderId,
                        x.EmployeeId,
                        x.Shop.Name,
                        x.Shop.ShopOwner,
                        x.Shop.Area,
                        CustomerName = x.Employee != null ? x.Employee.Name : string.Empty,
                        Date = x.Date.Date,
                        DueAmount = x.DueAmount,
                        IsPayment = false // Mark as due entry
                    })
                    .ToListAsync();

                // Get payment entries with customer name
                var payments = await _dbContext.DSRShopPaymentHistories
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.Customer)
                    .Select(x => new
                    {
                        x.ShopId,
                        x.Shop.Name,
                        x.CustomerId,
                        x.Shop.ShopOwner,
                        x.Shop.Area,
                        CustomerName = x.Customer != null ? x.Customer.Name : string.Empty,
                        Date = x.PaymentDate.Date,
                        PaidAmount = x.AmountPaid,
                        IsPayment = true // Mark as payment entry
                    })
                    .ToListAsync();

                // Create separate lists that maintain the IsPayment flag
                var dueSummaries = dues
                    .GroupBy(x => new { x.Date, x.ShopId, x.OrderId, x.EmployeeId })
                    .Select(g => new
                    {
                        Data = new TempShopDuePaymentListSummary
                        {
                            Date = g.Key.Date,
                            ShopId = g.Key.ShopId,
                            OrderId = g.Key.OrderId.Value,
                            CustomerId = g.Key.EmployeeId.Value,
                            ShopName = g.First().Name,
                            OwnerName = g.First().ShopOwner,
                            Area = g.First().Area,
                            ReferredBy = g.First().CustomerName,
                            DueAmount = g.Sum(x => x.DueAmount),
                            PaidAmount = 0
                        },
                        IsPayment = false
                    })
                    .ToList();

                var paymentSummaries = payments
                    .GroupBy(x => new { x.Date, x.ShopId, x.CustomerId })
                    .Select(g => new
                    {
                        Data = new TempShopDuePaymentListSummary
                        {
                            Date = g.Key.Date,
                            ShopId = g.Key.ShopId,
                            CustomerId = g.Key.CustomerId,
                            ShopName = g.First().Name,
                            OwnerName = g.First().ShopOwner,
                            Area = g.First().Area,
                            ReferredBy = g.First().CustomerName,
                            DueAmount = 0,
                            PaidAmount = g.Sum(x => x.PaidAmount)
                        },
                        IsPayment = true
                    })
                    .ToList();

                // Combine both lists and group
                var combined = dueSummaries.Concat(paymentSummaries)
                    .GroupBy(x => new { x.Data.Date, x.Data.ShopId, x.Data.CustomerId })
                    .Select(g => new TempShopDuePaymentListSummary
                    {
                        Date = g.Key.Date,
                        ShopId = g.Key.ShopId,
                        CustomerId = g.Key.CustomerId,
                        // For OrderId, take the first non-null one (if any)
                        OrderId = g.FirstOrDefault(x => x.Data.OrderId != null)?.Data.OrderId ?? 0,
                        ShopName = g.First().Data.ShopName,
                        OwnerName = g.First().Data.OwnerName,
                        Area = g.First().Data.Area,
                        // For IssuedBy, prioritize payment customer if available, otherwise use due customer
                        ReferredBy = g.FirstOrDefault(x => x.IsPayment && !string.IsNullOrEmpty(x.Data.ReferredBy))?.Data.ReferredBy
                                    ?? g.FirstOrDefault(x => !string.IsNullOrEmpty(x.Data.ReferredBy))?.Data.ReferredBy
                                    ?? string.Empty,
                        DueAmount = g.Sum(x => x.Data.DueAmount),
                        PaidAmount = g.Sum(x => x.Data.PaidAmount)
                    })
                    .ToList();

                // Map to final model
                var result = combined
                    .OrderByDescending(x => x.Date)
                    .ThenBy(x => x.ShopName)
                    .Select(x => new ShopDuePaymentListSummary
                    {
                        ShopId = x.ShopId,
                        ShopName = x.ShopName,
                        OrderId = x.OrderId,
                        CustomerId = x.CustomerId,
                        OwnerName = x.OwnerName,
                        Area = x.Area,
                        ReferredBy = x.ReferredBy,
                        Date = x.Date,
                        ShopDueAmount = x.DueAmount,
                        ShopPaidAmount = x.PaidAmount
                    })
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                return new List<ShopDuePaymentListSummary>();
            }
        }

        //public async Task<IList<ShopDuePaymentListSummary>> GetAllShopDuePaymentListSummary()
        //{
        //    try
        //    {
        //        // Get due entries with customer name
        //        var dues = await _dbContext.DSRShopDues
        //            .Include( x => x.Shop)
        //            .Where(x => !x.IsDeleted && !x.Shop.IsDeleted)
        //            .Include(x => x.Customer)
        //            .Select(x => new
        //            {
        //                x.ShopId,
        //                x.OrderId,
        //                x.CustomerId,
        //                x.Shop.Name,
        //                x.Shop.ShopOwner,
        //                x.Shop.Area,
        //                CustomerName = x.Customer != null ? x.Customer.Name : string.Empty,
        //                Date = x.Date.Date,
        //                DueAmount = x.DueAmount
        //            })
        //            .ToListAsync();

        //        // Get payment entries
        //        var payments = await _dbContext.DSRShopPaymentHistories
        //            .Where(x => !x.IsDeleted)
        //            .Select(x => new
        //            {
        //                x.ShopId,
        //                x.Shop.Name,
        //                x.CustomerId,
        //                x.Shop.ShopOwner,
        //                x.Shop.Area,
        //                Date = x.PaymentDate.Date,
        //                PaidAmount = x.AmountPaid
        //            })
        //            .ToListAsync();

        //        // Group dues by Date, ShopId, OrderId, CustomerId
        //        var dueGroups = dues
        //            .GroupBy(x => new { x.Date, x.ShopId, x.OrderId, x.CustomerId })
        //            .Select(g => new TempShopDuePaymentListSummary
        //            {
        //                Date = g.Key.Date,
        //                ShopId = g.Key.ShopId,
        //                OrderId = g.Key.OrderId.Value,
        //                CustomerId = g.Key.CustomerId.Value,
        //                ShopName = g.First().Name,
        //                OwnerName = g.First().ShopOwner,
        //                Area = g.First().Area,
        //                IssuedBy = g.First().CustomerName,
        //                DueAmount = g.Sum(x => x.DueAmount),
        //                PaidAmount = 0
        //            })
        //            .ToList();

        //        // Group payments by Date, ShopId, CustomerId
        //        var paymentGroups = payments
        //            .GroupBy(x => new { x.Date, x.ShopId, x.CustomerId })
        //            .Select(g => new TempShopDuePaymentListSummary
        //            {
        //                Date = g.Key.Date,
        //                ShopId = g.Key.ShopId,
        //                CustomerId = g.Key.CustomerId,
        //                ShopName = g.First().Name,
        //                OwnerName = g.First().ShopOwner,
        //                Area = g.First().Area,
        //                IssuedBy = string.Empty,
        //                DueAmount = 0,
        //                PaidAmount = g.Sum(x => x.PaidAmount)
        //            })
        //            .ToList();

        //        // Combine both lists - first group by Date, ShopId, CustomerId (without OrderId)
        //        var combined = dueGroups.Concat(paymentGroups)
        //            .GroupBy(x => new { x.Date, x.ShopId, x.CustomerId })
        //            .Select(g => new TempShopDuePaymentListSummary
        //            {
        //                Date = g.Key.Date,
        //                ShopId = g.Key.ShopId,
        //                CustomerId = g.Key.CustomerId,
        //                // For OrderId, take the first non-null one (if any)
        //                OrderId = (int)(g.FirstOrDefault(x => x.OrderId != null)?.OrderId),
        //                ShopName = g.First().ShopName,
        //                OwnerName = g.First().OwnerName,
        //                Area = g.First().Area,
        //                // For IssuedBy, take the first non-empty one
        //                IssuedBy = g.FirstOrDefault(x => !string.IsNullOrEmpty(x.IssuedBy))?.IssuedBy ?? string.Empty,
        //                DueAmount = g.Sum(x => x.DueAmount),
        //                PaidAmount = g.Sum(x => x.PaidAmount)
        //            })
        //            .ToList();

        //        // Map to final model
        //        var result = combined
        //            .OrderByDescending(x => x.Date)
        //            .ThenBy(x => x.ShopName)
        //            .Select(x => new ShopDuePaymentListSummary
        //            {
        //                ShopId = x.ShopId,
        //                ShopName = x.ShopName,
        //                OrderId = x.OrderId,
        //                CustomerId = x.CustomerId,
        //                OwnerName = x.OwnerName,
        //                Area = x.Area,
        //                IssuedBy = x.IssuedBy,
        //                Date = x.Date,
        //                ShopDueAmount = x.DueAmount,
        //                ShopPaidAmount = x.PaidAmount
        //            })
        //            .ToList();

        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        return new List<ShopDuePaymentListSummary>();
        //    }
        //}


    }
}