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

        //public DSRShopPaymentHistory GetDSRShopPaymentHistory(int id)
        //{
        //    try
        //    {
        //        return _dbContext.DSRShopPaymentHistories.FirstOrDefault(c => c.Id == id);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        public DSRShopPaymentHistory GetDSRShopPaymentHistory(int id)
        {
            return _dbContext.DSRShopPaymentHistories
                .Include(x => x.Products)
                    .ThenInclude(p => p.Product)   // ← ✅ Navigation load
                .Include(x => x.Shop)
                .Include(x => x.Employee)
                .Include(x => x.PaymentMethod)
                .FirstOrDefault(x => x.Id == id);
        }

        public async Task<IList<DSRShopPaymentHistory>> GetAllDSRShopPaymentHistory(int shopId)
        {
            try
            {
                return await _dbContext.DSRShopPaymentHistories
                    .Include(x => x.Products)
                    .Include(x => x.Employee)
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
                    .Include(x => x.Employee)
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
                // 1️⃣ Shop-level due (DSRShopDue.DueAmount)
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
                        TotalDueAmount = g.Sum(x => x.DueAmount)
                    })
                    .ToListAsync();

                // 2️⃣ Shop-level payments (DSRShopPaymentHistory.AmountPaid)
                var shopPayments = await _dbContext.DSRShopPaymentHistories
                    .Where(x => !x.IsDeleted)
                    .GroupBy(x => new { x.ShopId })
                    .Select(g => new
                    {
                        g.Key.ShopId,
                        TotalPaid = g.Sum(x => x.AmountPaid)
                    })
                    .ToListAsync();

                // 3️⃣ Cylinder-level due (ShopEmptyCylinderProduct.DueAmount)
                var cylinderDues = await _dbContext.Set<ShopEmptyCylinderProduct>()
                    .Include(p => p.DSRShopDue)
                        .ThenInclude(d => d.Shop)
                    .Where(p => !p.DSRShopDue.IsDeleted && !p.DSRShopDue.Shop.IsDeleted)
                    .GroupBy(p => new { p.DSRShopDue.ShopId })
                    .Select(g => new
                    {
                        ShopId = g.Key.ShopId,
                        TotalCylinderDue = g.Sum(p => p.DueAmount)
                    })
                    .ToListAsync();

                // 4️⃣ ✅ NEW: Cylinder-level payments (EmptyCylinderPaymentHistory.PaidAmount)
                var cylinderPayments = await _dbContext.Set<EmptyCylinderPaymentHistory>()
                    .Include(p => p.DSRShopPaymentHistory)
                        .ThenInclude(h => h.Shop)
                    .Where(p => p.DSRShopPaymentHistory != null
                                && !p.DSRShopPaymentHistory.IsDeleted
                                && !p.DSRShopPaymentHistory.Shop.IsDeleted)
                    .GroupBy(p => new { p.DSRShopPaymentHistory.ShopId })
                    .Select(g => new
                    {
                        ShopId = g.Key.ShopId,
                        TotalCylinderPaid = g.Sum(p => p.PaidAmount)
                    })
                    .ToListAsync();

                // 5️⃣ Combine all
                var result = discounts.Select(d =>
                {
                    var shopPaid = shopPayments.FirstOrDefault(p => p.ShopId == d.ShopId)?.TotalPaid ?? 0;
                    var cylinderDue = cylinderDues.FirstOrDefault(c => c.ShopId == d.ShopId)?.TotalCylinderDue ?? 0;
                    var cylinderPaid = cylinderPayments.FirstOrDefault(c => c.ShopId == d.ShopId)?.TotalCylinderPaid ?? 0;

                    return new DSRDueSummary
                    {
                        ShopId = d.ShopId,
                        ShopName = d.Name,
                        OwnerName = d.ShopOwner,
                        Area = d.Area,
                        Number = d.Number,

                        // Shop-level: due − paid
                        TotalShopDueAmount = d.TotalDueAmount - shopPaid,

                        // ✅ Cylinder-level: due − paid
                        TotalCylinderDueAmount = cylinderDue - cylinderPaid
                    };
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
                // 1️⃣ Shop-level due entries
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

                // 2️⃣ Cylinder-level due entries (ShopEmptyCylinderProduct)
                var cylinderDues = await _dbContext.Set<ShopEmptyCylinderProduct>()
                    .Include(p => p.DSRShopDue)
                        .ThenInclude(d => d.Shop)
                    .Where(p => !p.DSRShopDue.IsDeleted
                                && p.DSRShopDue.ShopId == shopId)
                    .Select(p => new
                    {
                        p.DSRShopDue.ShopId,
                        Name = p.DSRShopDue.Shop.Name,
                        ShopOwner = p.DSRShopDue.Shop.ShopOwner,
                        Area = p.DSRShopDue.Shop.Area,
                        Date = p.DSRShopDue.Date.Date,
                        CylinderDueAmount = p.DueAmount
                    })
                    .ToListAsync();

                // 3️⃣ Shop-level payment entries
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

                // 4️⃣ Cylinder-level payment entries
                // ✅ FIX: ShopEmptyCylinderProduct নেই, তাই DSRShopPaymentHistory থেকে Shop এ যেতে হবে
                var cylinderPayments = await _dbContext.EmptyCylinderPaymentHistories
                    .Include(p => p.DSRShopPaymentHistory)
                        .ThenInclude(h => h.Shop)
                    .Where(p => p.DSRShopPaymentHistory != null
                                && !p.DSRShopPaymentHistory.IsDeleted
                                && p.DSRShopPaymentHistory.ShopId == shopId)
                    .Select(p => new
                    {
                        ShopId = p.DSRShopPaymentHistory.ShopId,
                        Name = p.DSRShopPaymentHistory.Shop.Name,
                        ShopOwner = p.DSRShopPaymentHistory.Shop.ShopOwner,
                        Area = p.DSRShopPaymentHistory.Shop.Area,
                        Date = p.DSRShopPaymentHistory.PaymentDate.Date,
                        CylinderPaidAmount = p.PaidAmount
                    })
                    .ToListAsync();

                // 5️⃣ Seed allData from shop dues
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
                        PaidAmount = 0,
                        CylinderDueAmount = 0,
                        CylinderPaidAmount = 0
                    })
                    .ToList();

                // 6️⃣ Merge cylinder dues
                foreach (var cylGroup in cylinderDues.GroupBy(x => x.Date))
                {
                    var existing = allData.FirstOrDefault(x => x.Date == cylGroup.Key);
                    if (existing != null)
                    {
                        existing.CylinderDueAmount = cylGroup.Sum(x => x.CylinderDueAmount);
                    }
                    else
                    {
                        allData.Add(new TempShopSummary
                        {
                            Date = cylGroup.Key,
                            ShopId = cylGroup.First().ShopId,
                            ShopName = cylGroup.First().Name,
                            OwnerName = cylGroup.First().ShopOwner,
                            Area = cylGroup.First().Area,
                            DueAmount = 0,
                            PaidAmount = 0,
                            CylinderDueAmount = cylGroup.Sum(x => x.CylinderDueAmount),
                            CylinderPaidAmount = 0
                        });
                    }
                }

                // 7️⃣ Merge shop payments
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
                            PaidAmount = paymentGroup.Sum(x => x.PaidAmount),
                            CylinderDueAmount = 0,
                            CylinderPaidAmount = 0
                        });
                    }
                }

                // 8️⃣ Merge cylinder payments
                foreach (var cylPayGroup in cylinderPayments.GroupBy(x => x.Date))
                {
                    var existing = allData.FirstOrDefault(x => x.Date == cylPayGroup.Key);
                    if (existing != null)
                    {
                        existing.CylinderPaidAmount = cylPayGroup.Sum(x => x.CylinderPaidAmount);
                    }
                    else
                    {
                        allData.Add(new TempShopSummary
                        {
                            Date = cylPayGroup.Key,
                            ShopId = cylPayGroup.First().ShopId,
                            ShopName = cylPayGroup.First().Name,
                            OwnerName = cylPayGroup.First().ShopOwner,
                            Area = cylPayGroup.First().Area,
                            DueAmount = 0,
                            PaidAmount = 0,
                            CylinderDueAmount = 0,
                            CylinderPaidAmount = cylPayGroup.Sum(x => x.CylinderPaidAmount)
                        });
                    }
                }

                // 9️⃣ Map to final model
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
                        CylinderDueAmount = x.CylinderDueAmount,
                        ShopPaidAmount = x.PaidAmount,
                        CylinderPaidAmount = x.CylinderPaidAmount
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
                // 1️⃣ Shop-level due entries with customer name
                var dues = await _dbContext.DSRShopDues
                    .Include(x => x.Shop)
                    .Include(x => x.Employee)
                    .Where(x => !x.IsDeleted && !x.Shop.IsDeleted)
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
                        IsPayment = false
                    })
                    .ToListAsync();

                // 2️⃣ ✅ NEW: Cylinder-level due entries
                var cylinderDues = await _dbContext.Set<ShopEmptyCylinderProduct>()
                    .Include(p => p.DSRShopDue)
                        .ThenInclude(d => d.Shop)
                    .Include(p => p.DSRShopDue)
                        .ThenInclude(d => d.Employee)
                    .Where(p => !p.DSRShopDue.IsDeleted && !p.DSRShopDue.Shop.IsDeleted)
                    .Select(p => new
                    {
                        p.DSRShopDue.ShopId,
                        p.DSRShopDue.OrderId,
                        p.DSRShopDue.EmployeeId,
                        Name = p.DSRShopDue.Shop.Name,
                        ShopOwner = p.DSRShopDue.Shop.ShopOwner,
                        Area = p.DSRShopDue.Shop.Area,
                        CustomerName = p.DSRShopDue.Employee != null ? p.DSRShopDue.Employee.Name : string.Empty,
                        Date = p.DSRShopDue.Date.Date,
                        CylinderDueAmount = p.DueAmount
                    })
                    .ToListAsync();

                // 3️⃣ Shop-level payment entries with customer name
                var payments = await _dbContext.DSRShopPaymentHistories
                    .Include(x => x.Shop)
                    .Include(x => x.Employee)
                    .Where(x => !x.IsDeleted)
                    .Select(x => new
                    {
                        x.ShopId,
                        x.Shop.Name,
                        x.EmployeeId,
                        x.Shop.ShopOwner,
                        x.Shop.Area,
                        CustomerName = x.Employee != null ? x.Employee.Name : string.Empty,
                        Date = x.PaymentDate.Date,
                        PaidAmount = x.AmountPaid,
                        IsPayment = true
                    })
                    .ToListAsync();

                // 4️⃣ ✅ NEW: Cylinder-level payment entries
                var cylinderPayments = await _dbContext.EmptyCylinderPaymentHistories
                    .Include(p => p.DSRShopPaymentHistory)
                        .ThenInclude(h => h.Shop)
                    .Include(p => p.DSRShopPaymentHistory)
                        .ThenInclude(h => h.Employee)
                    .Where(p => p.DSRShopPaymentHistory != null
                                && !p.DSRShopPaymentHistory.IsDeleted)
                    .Select(p => new
                    {
                        ShopId = p.DSRShopPaymentHistory.ShopId,
                        Name = p.DSRShopPaymentHistory.Shop.Name,
                        EmployeeId = p.DSRShopPaymentHistory.EmployeeId,
                        ShopOwner = p.DSRShopPaymentHistory.Shop.ShopOwner,
                        Area = p.DSRShopPaymentHistory.Shop.Area,
                        CustomerName = p.DSRShopPaymentHistory.Employee != null
                                        ? p.DSRShopPaymentHistory.Employee.Name
                                        : string.Empty,
                        Date = p.DSRShopPaymentHistory.PaymentDate.Date,
                        CylinderPaidAmount = p.PaidAmount
                    })
                    .ToListAsync();

                // 5️⃣ Build dueSummaries (shop-level dues)
                var dueSummaries = dues
                    .GroupBy(x => new { x.Date, x.ShopId, x.OrderId, x.EmployeeId })
                    .Select(g => new
                    {
                        Data = new TempShopDuePaymentListSummary
                        {
                            Date = g.Key.Date,
                            ShopId = g.Key.ShopId,
                            OrderId = g.Key.OrderId ?? 0,
                            EmployeeId = g.Key.EmployeeId ?? 0,
                            ShopName = g.First().Name,
                            OwnerName = g.First().ShopOwner,
                            Area = g.First().Area,
                            ReferredBy = g.First().CustomerName,
                            DueAmount = g.Sum(x => x.DueAmount),
                            PaidAmount = 0,
                            CylinderDueAmount = 0,
                            CylinderPaidAmount = 0
                        },
                        IsPayment = false
                    })
                    .ToList();

                // 6️⃣ ✅ NEW: Build cylinderDueSummaries
                var cylinderDueSummaries = cylinderDues
                    .GroupBy(x => new { x.Date, x.ShopId, x.OrderId, x.EmployeeId })
                    .Select(g => new
                    {
                        Data = new TempShopDuePaymentListSummary
                        {
                            Date = g.Key.Date,
                            ShopId = g.Key.ShopId,
                            OrderId = g.Key.OrderId ?? 0,
                            EmployeeId = g.Key.EmployeeId ?? 0,
                            ShopName = g.First().Name,
                            OwnerName = g.First().ShopOwner,
                            Area = g.First().Area,
                            ReferredBy = g.First().CustomerName,
                            DueAmount = 0,
                            PaidAmount = 0,
                            CylinderDueAmount = g.Sum(x => x.CylinderDueAmount),
                            CylinderPaidAmount = 0
                        },
                        IsPayment = false
                    })
                    .ToList();

                // 7️⃣ Build paymentSummaries (shop-level payments)
                var paymentSummaries = payments
                    .GroupBy(x => new { x.Date, x.ShopId, x.EmployeeId })
                    .Select(g => new
                    {
                        Data = new TempShopDuePaymentListSummary
                        {
                            Date = g.Key.Date,
                            ShopId = g.Key.ShopId,
                            OrderId = 0,
                            EmployeeId = g.Key.EmployeeId,
                            ShopName = g.First().Name,
                            OwnerName = g.First().ShopOwner,
                            Area = g.First().Area,
                            ReferredBy = g.First().CustomerName,
                            DueAmount = 0,
                            PaidAmount = g.Sum(x => x.PaidAmount),
                            CylinderDueAmount = 0,
                            CylinderPaidAmount = 0
                        },
                        IsPayment = true
                    })
                    .ToList();

                // 8️⃣ ✅ NEW: Build cylinderPaymentSummaries
                var cylinderPaymentSummaries = cylinderPayments
                    .GroupBy(x => new { x.Date, x.ShopId, x.EmployeeId })
                    .Select(g => new
                    {
                        Data = new TempShopDuePaymentListSummary
                        {
                            Date = g.Key.Date,
                            ShopId = g.Key.ShopId,
                            OrderId = 0,
                            EmployeeId = g.Key.EmployeeId,
                            ShopName = g.First().Name,
                            OwnerName = g.First().ShopOwner,
                            Area = g.First().Area,
                            ReferredBy = g.First().CustomerName,
                            DueAmount = 0,
                            PaidAmount = 0,
                            CylinderDueAmount = 0,
                            CylinderPaidAmount = g.Sum(x => x.CylinderPaidAmount)
                        },
                        IsPayment = true
                    })
                    .ToList();

                // 9️⃣ Combine all four lists and group
                var combined = dueSummaries
                    .Concat(cylinderDueSummaries)
                    .Concat(paymentSummaries)
                    .Concat(cylinderPaymentSummaries)
                    .GroupBy(x => new { x.Data.Date, x.Data.ShopId, x.Data.EmployeeId })
                    .Select(g => new TempShopDuePaymentListSummary
                    {
                        Date = g.Key.Date,
                        ShopId = g.Key.ShopId,
                        EmployeeId = g.Key.EmployeeId,
                        // For OrderId, take the first non-zero one
                        OrderId = g.FirstOrDefault(x => x.Data.OrderId != 0)?.Data.OrderId ?? 0,
                        ShopName = g.First().Data.ShopName,
                        OwnerName = g.First().Data.OwnerName,
                        Area = g.First().Data.Area,
                        // For ReferredBy, prioritize payment customer if available
                        ReferredBy = g.FirstOrDefault(x => x.IsPayment && !string.IsNullOrEmpty(x.Data.ReferredBy))?.Data.ReferredBy
                                    ?? g.FirstOrDefault(x => !string.IsNullOrEmpty(x.Data.ReferredBy))?.Data.ReferredBy
                                    ?? string.Empty,
                        DueAmount = g.Sum(x => x.Data.DueAmount),
                        PaidAmount = g.Sum(x => x.Data.PaidAmount),
                        CylinderDueAmount = g.Sum(x => x.Data.CylinderDueAmount),
                        CylinderPaidAmount = g.Sum(x => x.Data.CylinderPaidAmount)
                    })
                    .ToList();

                // 🔟 Map to final model
                var result = combined
                    .OrderByDescending(x => x.Date)
                    .ThenBy(x => x.ShopName)
                    .Select(x => new ShopDuePaymentListSummary
                    {
                        ShopId = x.ShopId,
                        ShopName = x.ShopName,
                        OrderId = x.OrderId,
                        EmployeeId = x.EmployeeId,
                        OwnerName = x.OwnerName,
                        Area = x.Area,
                        ReferredBy = x.ReferredBy,
                        Date = x.Date,
                        ShopDueAmount = x.DueAmount,
                        ShopPaidAmount = x.PaidAmount,
                        CylinderDueAmount = x.CylinderDueAmount,
                        CylinderPaidAmount = x.CylinderPaidAmount
                    })
                    .ToList();

                return result;
            }
            catch (Exception)
            {
                return new List<ShopDuePaymentListSummary>();
            }
        }

        public void AddEmptyCylinderPaymentHistory(EmptyCylinderPaymentHistory product)
        {
            _dbContext.EmptyCylinderPaymentHistories.Add(product);
            _dbContext.SaveChanges();
        }

        public void DeleteEmptyCylinderPaymentHistory(int id)
        {
            var entity = _dbContext.EmptyCylinderPaymentHistories.Find(id);
            if (entity != null)
            {
                _dbContext.EmptyCylinderPaymentHistories.Remove(entity);
                _dbContext.SaveChanges();
            }
        }

    }
}