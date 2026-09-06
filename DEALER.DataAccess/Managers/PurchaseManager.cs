namespace DEALER.DataAccess
{
    public class PurchaseManager : BaseDataManager, IPurchase
    {
        public PurchaseManager(DEALERContext model) : base(model)
        {
        }


        public bool CreatePurchase(Purchase purchase)
        {
            if (purchase == null || purchase.PurchaseDetails == null || !purchase.PurchaseDetails.Any())
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                _dbContext.Purchases.Add(purchase);
                _dbContext.SaveChanges(); // Save to generate Purchase ID

                foreach (var item in purchase.PurchaseDetails)
                {
                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == item.ProductId);
                    if (stock != null)
                    {
                        ApplyStockForPurchase(stock, item);
                    }
                }
                _dbContext.SaveChanges();

                var lastPayment = _dbContext.SupplierPaymentHistories
                                 .Where(p => p.SupplierId == purchase.SupplierId && !p.IsDeleted)
                                 .OrderByDescending(p => p.Id)
                                 .FirstOrDefault();

                double totalDueBefore = lastPayment?.TotalDueAfterPayment ?? 0;
                double totalDueAfter = totalDueBefore + purchase.TotalAmount;

                var payment = new SupplierPaymentHistory()
                {
                    SupplierId = purchase.SupplierId,
                    PurchaseId = purchase.Id,
                    PaymentDate = purchase.Date,
                    Comments = purchase.Comments,
                    PaymentMethod = purchase.PaymentMethod,
                    TransactionID = purchase.TransactionID,
                    Number = purchase.Number,
                    TotalAmountThisPurchase = purchase.TotalAmount,
                    AmountPaid = purchase.TotalPay,
                    TotalDueBeforePayment = totalDueBefore,
                    TotalDueAfterPayment = totalDueAfter - purchase.TotalPay
                };

                _dbContext.SupplierPaymentHistories.Add(payment);
                _dbContext.SaveChanges();

                if (purchase.TotalPay > 0)
                {
                    var existCurrentBalance = _dbContext.TransactionHistories
                                           .AsNoTracking()
                                           .Where(x => !x.IsDeleted)
                                           .OrderByDescending(x => x.Id)
                                           .FirstOrDefault()?.CurrentBalance ?? 0;

                    TransactionHistory transactionHistory = new()
                    {
                        BalanceIn = 0,
                        BalanceOut = purchase.TotalPay,
                        CurrentBalance = existCurrentBalance - purchase.TotalPay,
                        Date = purchase.Date,
                        PurchaseId = purchase.Id,
                        Resone = purchase.Supplier?.Name != null ? $"Purchase from {purchase.Supplier.Name}." : $"Purchase Payment",
                    };

                    _dbContext.Add(transactionHistory);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public bool UpdatePurchase(Purchase purchase)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var existingPurchaseData = _dbContext.Purchases
                   .Include(o => o.PurchaseDetails)
                   .AsNoTracking()
                   .FirstOrDefault(o => o.Id == purchase.Id);

                var existingPurchase = _dbContext.Purchases
                    .Include(o => o.PurchaseDetails)
                    .FirstOrDefault(o => o.Id == purchase.Id);

                if (existingPurchase == null)
                    return false;

                // --- Step 1: পুরনো purchase details এর stock effect আগে reverse করো ---
                var oldDetails = existingPurchaseData?.PurchaseDetails ?? new List<PurchaseDetail>();
                foreach (var oldDetail in oldDetails)
                {
                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == oldDetail.ProductId);
                    if (stock != null)
                    {
                        ReverseStockForPurchase(stock, oldDetail);
                    }
                }

                // Update purchase header
                existingPurchase.PaymentMethod = purchase.PaymentMethod;
                existingPurchase.ShippingMethod = purchase.ShippingMethod;
                existingPurchase.DeliveryCharge = purchase.DeliveryCharge;
                existingPurchase.TotalAmount = purchase.TotalAmount;
                existingPurchase.TotalPay = purchase.TotalPay;
                existingPurchase.Comments = purchase.Comments;
                existingPurchase.TotalDue = purchase.TotalDue;
                existingPurchase.Date = purchase.Date;
                existingPurchase.PurchaseDetails = purchase.PurchaseDetails;

                _dbContext.SaveChanges();

                // --- Step 2: নতুন purchase details এর stock effect apply করো ---
                foreach (var newDetail in existingPurchase.PurchaseDetails)
                {
                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == newDetail.ProductId);
                    if (stock != null)
                    {
                        ApplyStockForPurchase(stock, newDetail);
                    }
                }

                _dbContext.SaveChanges();

                // Update supplier payment history
                var existingPayment = _dbContext.SupplierPaymentHistories
                    .FirstOrDefault(p => p.PurchaseId == purchase.Id);

                var amountDifference = existingPurchaseData.TotalPay - purchase.TotalPay;
                var purchaseDifference = existingPurchaseData.TotalAmount - purchase.TotalAmount;

                if (existingPayment != null)
                {
                    existingPayment.PaymentMethod = purchase.PaymentMethod;
                    existingPayment.PaymentDate = purchase.Date;
                    existingPayment.Number = purchase.Number;
                    existingPayment.Comments = purchase.Comments;
                    existingPayment.TransactionID = purchase.TransactionID;
                    existingPayment.AmountPaid = purchase.TotalPay;
                    existingPayment.TotalDueAfterPayment = (existingPayment.TotalDueAfterPayment + amountDifference) - purchaseDifference;
                    existingPayment.TotalAmountThisPurchase = purchase.TotalAmount;

                    RecalculateSupplierPaymentHistoriesAsync(existingPayment.SupplierId, existingPayment.Id, purchaseDifference);

                    _dbContext.SaveChanges();
                }

                var existingTransaction = _dbContext.TransactionHistories
                    .FirstOrDefault(x => x.PurchaseId == purchase.Id);

                if (existingTransaction != null && purchase.TotalPay != existingPurchaseData.TotalPay)
                {
                    existingTransaction.Date = purchase.Date;
                    existingTransaction.BalanceOut -= amountDifference;
                    existingTransaction.CurrentBalance += amountDifference;

                    if (amountDifference != 0)
                    {
                        BalanceInTransactionHistories(existingTransaction.Id, amountDifference);
                    }
                }

                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public bool DeletePurchase(int id)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var purchasesEntity = _dbContext.Purchases.FirstOrDefault(x => x.Id == id);

                if (purchasesEntity == null)
                {
                    return false;
                }

                if (purchasesEntity.IsDeleted)
                    return true; // আগে থেকেই deleted, double-reverse এড়ানো

                purchasesEntity.IsDeleted = true;
                _dbContext.Update(purchasesEntity);
                _dbContext.SaveChanges();

                var purchasesDetailsEntity = _dbContext.PurchaseDetails.Where(x => x.PurchaseId == id).ToList();

                foreach (var detail in purchasesDetailsEntity)
                {
                    var stock = _dbContext.ProductStocks.FirstOrDefault(x => x.ProductId == detail.ProductId);
                    if (stock != null)
                    {
                        ReverseStockForPurchase(stock, detail);
                    }
                }
                _dbContext.SaveChanges();

                var supplierPaymentHistoriesEntity = _dbContext.SupplierPaymentHistories
                    .FirstOrDefault(p => p.PurchaseId == purchasesEntity.Id
                    && p.SupplierId == purchasesEntity.SupplierId);

                if (supplierPaymentHistoriesEntity is not null)
                {
                    supplierPaymentHistoriesEntity.IsDeleted = true;

                    var amount = purchasesEntity.TotalAmount - supplierPaymentHistoriesEntity.AmountPaid;
                    RecalculateSupplierPaymentHistoriesAsync(supplierPaymentHistoriesEntity.SupplierId, supplierPaymentHistoriesEntity.Id, amount);

                    _dbContext.Update(supplierPaymentHistoriesEntity);
                    _dbContext.SaveChanges();
                }

                var purchasestransactionHistory = _dbContext.TransactionHistories
                    .FirstOrDefault(t => t.PurchaseId == purchasesEntity.Id);

                if (purchasestransactionHistory is not null)
                {
                    if (purchasestransactionHistory.BalanceOut.HasValue
                        && purchasestransactionHistory.BalanceOut.Value > 0)
                    {
                        BalanceInTransactionHistories(purchasestransactionHistory.Id, purchasestransactionHistory.BalanceOut.Value);
                    }

                    purchasestransactionHistory.IsDeleted = true;
                    _dbContext.Update(purchasestransactionHistory);
                    _dbContext.SaveChanges();
                }

                if (supplierPaymentHistoriesEntity is not null)
                {
                    var supplierPaymentHistorytransactionHistory = _dbContext.TransactionHistories
                    .FirstOrDefault(t => t.SupplierPaymentHistoryId == supplierPaymentHistoriesEntity.Id);

                    if (supplierPaymentHistorytransactionHistory is not null)
                    {
                        if (supplierPaymentHistorytransactionHistory.BalanceOut.HasValue
                           && supplierPaymentHistorytransactionHistory.BalanceOut.Value > 0)
                        {
                            BalanceOutTransactionHistories(supplierPaymentHistorytransactionHistory.Id, supplierPaymentHistorytransactionHistory.BalanceOut.Value);
                        }

                        supplierPaymentHistorytransactionHistory.IsDeleted = true;
                        _dbContext.Update(supplierPaymentHistorytransactionHistory);
                        _dbContext.SaveChanges();
                    }
                }
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // --- Helper methods ---

        private void ApplyStockForPurchase(ProductStock stock, PurchaseDetail detail)
        {
            stock.LoadedQty += detail.Quantity;
            stock.EmptyQty -= detail.ReturnQuantity ?? 0;
        }

        private void ReverseStockForPurchase(ProductStock stock, PurchaseDetail detail)
        {
            stock.LoadedQty -= detail.Quantity;
            stock.EmptyQty += detail.ReturnQuantity ?? 0;
        }

        private void RecalculateSupplierPaymentHistoriesAsync(int supplierId, int id, double amount)
        {
            if (amount == 0)
                return;

            try
            {
                var payments = _dbContext.SupplierPaymentHistories
                                   .Where(p => p.SupplierId == supplierId && p.Id > id)
                                   .OrderBy(p => p.Id)
                                   .ToList();

                if (!payments.Any())
                {
                    return;
                }

                foreach (var payment in payments)
                {
                    payment.TotalDueBeforePayment -= amount;
                    payment.TotalDueAfterPayment -= amount;
                }

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<Purchase> GetPurchaseById(int purchaseId)
        {
            try
            {
                return await _dbContext.Purchases
                .Include(o => o.PurchaseDetails)
                .FirstOrDefaultAsync(o => o.Id == purchaseId && !o.IsDeleted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<IEnumerable<PurchasesDetailsDTO>> GetPurchasesDetailsByOrderAsync(int purchaseId)
        {
            try
            {
                var purchaseDetails = await _dbContext.PurchaseDetails
                    .Include(pd => pd.Product)
                        .ThenInclude(p => p.ProductsSize)
                    .Include(pd => pd.Product)
                        .ThenInclude(p => p.Supplier)
                    .Where(pd => pd.PurchaseId == purchaseId)
                    .Select(pd => new PurchasesDetailsDTO
                    {
                        ProductName = pd.Product.DisplayNameSize,
                        SupplierName = pd.Product.Supplier.Name,
                        Quantity = pd.Quantity,
                        ReturnQuantity = pd.ReturnQuantity ?? 0,
                        ProductPrice = pd.GasUnitPrice,
                        Discount = pd.Discount,
                        TotalPrice = (double)((pd.Quantity * pd.GasUnitPrice) - (pd.Discount + (pd.ReturnQuantity ?? 0) * pd.CylinderUnitPrice))
                    })
                    .ToListAsync();

                return purchaseDetails;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }
    }

    public class ProductQuantityDifference
    {
        public int ProductId { get; set; }
        public int QuantityDifference { get; set; }
    }
}