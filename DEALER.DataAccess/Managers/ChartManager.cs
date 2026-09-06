namespace DEALER.DataAccess
{
    public class ChartManager : BaseDataManager, IChart
    {
        public ChartManager(DEALERContext model) : base(model)
        {
        }

        #region Last 30 Days Sales History

        //public IList<BarGraphDTO> Get30DaysSalesHistory(DateTime? fromDate, DateTime? toDate)
        //{
        //    // Determine the date range
        //    var startDate = fromDate?.Date ?? DateTime.Now.Date.AddDays(-30);
        //    var endDate = toDate?.Date ?? DateTime.Now.Date;

        //    var salesData = _dbContext.Orders
        //        .Where(o => o.Date.Date >= startDate && o.Date.Date <= endDate && !o.IsDeleted)
        //        .SelectMany(o => o.OrderDetails)
        //        .Select(pd => new
        //        {
        //            SaleDate = pd.Order.Date.Date,
        //            Quantity = pd.Quantity,
        //            ReturnQuantity = (pd.ReturnQuantity ?? 0),
        //            UnitPrice = pd.UnitPrice
        //        })
        //        .GroupBy(x => x.SaleDate)
        //        .Select(g => new BarGraphDTO
        //        {
        //            MonthName = g.Key.ToString("MMM-dd"),
        //            Amount = (decimal)Math.Round(g.Sum(x => (x.Quantity - x.ReturnQuantity) * x.UnitPrice), 2)
        //        })
        //        .ToList();

        //    return salesData;
        //}

        #endregion Last 30 Days Sales History

        #region Last 30 Days Daily Expense History

        public IList<BarGraphDTO> Get30DaysDailyExpenseHistory(DateTime? fromDate, DateTime? toDate)
        {
            // Determine the date range with proper validation
            var endDate = (toDate?.Date ?? DateTime.Now.Date).AddDays(1); // Add a day for exclusive comparison
            var startDate = fromDate?.Date ?? endDate.AddDays(-30);

            if (startDate > endDate)
            {
                (startDate, endDate) = (endDate, startDate);
            }

            var expenseHistory = _dbContext.DailyExpenses
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.Date >= startDate && o.Date < endDate)
                .GroupBy(pd => pd.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToList() // move data to memory
                .Select(g => new BarGraphDTO
                {
                    MonthName = g.Date.ToString("MMM-dd"),
                    Amount = (decimal)Math.Round(g.Amount, 2)
                })
                .OrderBy(g => g.MonthName) // Optional: keep the correct date order
                .ToList();

            return expenseHistory;
        }


        #endregion Last 30 Days Daily Expense History

        #region Last 30 Days Profit History

        //public IList<BarGraphDTO> GetLast30DaysProfitHistory(DateTime? fromDate, DateTime? toDate)
        //{
        //    // Step 1: Determine date range
        //    var startDate = fromDate?.Date ?? DateTime.Now.Date.AddDays(-30);
        //    var endDate = toDate?.Date ?? DateTime.Now.Date;

        //    // Step 2: Get all relevant orders and compute profit per order
        //    var orderProfits = (from o in _dbContext.Orders
        //                        where o.Date.Date >= startDate && o.Date.Date <= endDate && !o.IsDeleted
        //                        let totalCost = (from pd in _dbContext.OrderDetails
        //                                         join p in _dbContext.Products on pd.ProductId equals p.Id
        //                                         where pd.OrderId == o.Id
        //                                         select (pd.Quantity - (pd.ReturnQuantity ?? 0)) * (pd.UnitPrice - pd.BuyingPrice)).Sum()
        //                        select new
        //                        {
        //                            Date = o.Date.Date,
        //                            Profit = totalCost - o.Discount
        //                        })
        //                        .ToList();

        //    // Step 3: Get all daily expenses from the selected date range
        //    var dailyExpenses = _dbContext.DailyExpenses
        //                          .Where(e => !e.IsDeleted && e.Date.Date >= startDate && e.Date.Date <= endDate)
        //                          .GroupBy(e => e.Date.Date)
        //                          .Select(g => new
        //                          {
        //                              Date = g.Key,
        //                              TotalExpense = g.Sum(x => x.Amount)
        //                          })
        //                          .ToList();

        //    // Step 4: Group profits by day
        //    var dailyProfits = orderProfits
        //        .GroupBy(x => x.Date)
        //        .Select(g => new
        //        {
        //            Date = g.Key,
        //            TotalProfit = g.Sum(x => x.Profit)
        //        })
        //        .ToList();

        //    // Step 5: Create list of dates in the range
        //    var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
        //                     .Select(i => startDate.AddDays(i))
        //                     .ToList();

        //    // Step 6: Combine profits and expenses
        //    var result = allDates
        //        .Select(date =>
        //        {
        //            var profit = dailyProfits.FirstOrDefault(p => p.Date == date)?.TotalProfit ?? 0;
        //            var expense = dailyExpenses.FirstOrDefault(e => e.Date == date)?.TotalExpense ?? 0;

        //            return new BarGraphDTO
        //            {
        //                MonthName = date.ToString("MMM-dd"),
        //                Amount = (decimal)Math.Round(profit - expense, 2)
        //            };
        //        })
        //        .Where(x => x.Amount > 1)
        //        .OrderBy(r => DateTime.ParseExact(r.MonthName, "MMM-dd", null))
        //        .ToList();

        //    return result;
        //}

        #endregion Last 30 Days Profit History

        #region Last 30 Days Product Sales History

        public async Task<IList<ProductSalesDTO>> LoadProductSales(int productId, DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate ?? DateTime.Now.AddDays(-30);
            var endDate = toDate ?? DateTime.Now;

            var productsList = await _dbContext.OrderDetails
                .Where(od => od.ProductId == productId &&
                             od.Order!.Date >= startDate &&
                             od.Order.Date <= endDate &&
                             !od.Order.IsDeleted)
                .AsNoTracking()
                .GroupBy(od => od.Order.Date.Date)
                .Select(g => new ProductSalesDTO
                {
                    Date = g.Key,
                    FormattedDate = g.Key.ToString("MMM dd"),
                    QuantitySold = g.Sum(od => (od.Quantity - (od.ReturnQuantity ?? 0)))
                })
                .OrderBy(ps => ps.Date)
                .ToListAsync();

            return productsList;
        }
        #endregion Last 30 Days Product Sales History


        #region Last 12 Month Expense History

        public IList<BarGraphDTO> GetLast12MonthExpenseHistory(DateTime? fromDate, DateTime? toDate)
        {
            // Step 1: Determine date range (default = last 12 months)
            var endDate = (toDate?.Date ?? DateTime.Now.Date).AddDays(1); // exclusive
            var startDate = fromDate?.Date ?? endDate.AddMonths(-12);

            // Step 2: Query DailyExpenses
            var dailyExpenseQuery = _dbContext.DailyExpenses
                .AsNoTracking()
                .Where(e => !e.IsDeleted && e.Date >= startDate && e.Date < endDate)
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(x => x.Amount)
                });

            // Step 3: Query Expenses
            var expenseQuery = _dbContext.Expenses
                .AsNoTracking()
                .Where(e => !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate < endDate)
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(x => x.Amount)
                });

            // Step 4: Combine both sources
            var combinedData = dailyExpenseQuery
                .Concat(expenseQuery)
                .ToList()
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new BarGraphDTO
                {
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Amount = (decimal)g.Sum(x => Math.Round(x.Amount, 2))
                })
                .OrderBy(x => DateTime.ParseExact(x.MonthName, "MMM yyyy", null))
                .ToList();

            return combinedData;
        }

        public IList<BarGraphDTO> GetLast30DaysProfitHistory(DateTime? fromDate, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        public IList<BarGraphDTO> Get30DaysSalesHistory(DateTime? fromDate, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        public IList<BarGraphDTO> GetMonthlyProfitHistory(DateTime? fromDate, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        public IList<BarGraphDTO> GetLast12MonthSalesHistory(int supplierId)
        {
            throw new NotImplementedException();
        }

        public IList<BarGraphDTO> GetLast30DaysSalesHistory(int supplierId)
        {
            throw new NotImplementedException();
        }


        #endregion Last 12 Month Expense History

        #region Monthly Profit History


        //public IList<BarGraphDTO> GetMonthlyProfitHistory(DateTime? fromDate, DateTime? toDate)
        //{
        //    // Step 1: Date range
        //    var endDate = (toDate?.Date ?? DateTime.Now.Date).AddDays(1); // exclusive
        //    var startDate = fromDate?.Date ?? endDate.AddMonths(-12);


        //    var orderProfits = (from o in _dbContext.Orders
        //                        join d in _dbContext.OrderDetails on o.Id equals d.OrderId
        //                        join p in _dbContext.Products on d.ProductId equals p.Id
        //                        where o.Date >= startDate && o.Date < endDate && !o.IsDeleted
        //                        group new { d, p, o } by new { o.Date.Year, o.Date.Month, o.Id, o.Discount } into g
        //                        select new
        //                        {
        //                            Year = g.Key.Year,
        //                            Month = g.Key.Month,
        //                            OrderId = g.Key.Id,
        //                            TotalProfit = g.Sum(x => (x.d.Quantity - (x.d.ReturnQuantity ?? 0)) * (x.d.UnitPrice - x.d.BuyingPrice)) - g.Key.Discount
        //                        })
        //            .GroupBy(x => new { x.Year, x.Month })
        //            .Select(g => new
        //            {
        //                Year = g.Key.Year,
        //                Month = g.Key.Month,
        //                TotalProfit = g.Sum(x => x.TotalProfit)
        //            })
        //            .ToList();

        //    // Step 3: Get monthly DailyExpenses
        //    var dailyExpenses = _dbContext.DailyExpenses
        //        .Where(e => !e.IsDeleted && e.Date >= startDate && e.Date < endDate)
        //        .GroupBy(e => new { e.Date.Year, e.Date.Month })
        //        .Select(g => new
        //        {
        //            Year = g.Key.Year,
        //            Month = g.Key.Month,
        //            TotalExpense = g.Sum(x => x.Amount)
        //        }).ToList();

        //    // Step 4: Get monthly Expenses
        //    var monthlyExpenses = _dbContext.Expenses
        //        .Where(e => !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate < endDate)
        //        .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
        //        .Select(g => new
        //        {
        //            Year = g.Key.Year,
        //            Month = g.Key.Month,
        //            TotalExpense = g.Sum(x => x.Amount)
        //        }).ToList();

        //    // Step 5: Combine all months
        //    var allMonths = orderProfits
        //        .Select(x => new { x.Year, x.Month })
        //        .Union(dailyExpenses.Select(x => new { x.Year, x.Month }))
        //        .Union(monthlyExpenses.Select(x => new { x.Year, x.Month }))
        //        .Distinct()
        //        .OrderBy(x => new DateTime(x.Year, x.Month, 1))
        //        .ToList();

        //    // Step 6: Build final list
        //    var result = allMonths.Select(m =>
        //    {
        //        var profit = orderProfits.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.TotalProfit ?? 0;
        //        var dailyExp = dailyExpenses.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.TotalExpense ?? 0;
        //        var otherExp = monthlyExpenses.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.TotalExpense ?? 0;

        //        return new BarGraphDTO
        //        {
        //            MonthName = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
        //            Amount = (decimal)Math.Round(profit - dailyExp - otherExp, 2)
        //        };
        //    }).Where(x => x.Amount != 0).ToList();

        //    return result;
        //}

        #endregion Monthly Profit History

        //public IList<BarGraphDTO> GetLast12MonthSalesHistory(int supplierId)
        //{
        //    var fromDate = DateTime.Now.AddMonths(-11).Date;
        //    var toDate = DateTime.Now;


        //    // Step 1: Perform group and sum in SQL
        //    var salesRaw = _dbContext.OrderDetails
        //        .Where(od => od.Order.Date >= fromDate
        //        && od.Order.Date <= toDate && !od.Order.IsDeleted
        //        && od.Product.SupplierId == supplierId)
        //        .GroupBy(od => new { od.Order.Date.Year, od.Order.Date.Month })
        //        .Select(g => new
        //        {
        //            Year = g.Key.Year,
        //            Month = g.Key.Month,
        //            Amount = g.Sum(x => (x.Quantity - (x.ReturnQuantity ?? 0)) * x.UnitPrice)
        //        })
        //        .ToList(); // Bring data into memory

        //    // 🔁 Return early if no data
        //    if (salesRaw == null || !salesRaw.Any())
        //        return new List<BarGraphDTO>();

        //    // Step 2: Create all last 12 months
        //    var allMonths = Enumerable.Range(0, 12)
        //        .Select(i => DateTime.Now.AddMonths(-i))
        //        .Select(d => new DateTime(d.Year, d.Month, 1))
        //        .OrderBy(d => d)
        //        .ToList();

        //    // Step 3: Merge raw data with month list
        //    var result = allMonths.Select(d =>
        //    {
        //        var match = salesRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
        //        return new BarGraphDTO
        //        {
        //            MonthName = d.ToString("MMM yy"),
        //            Amount = Math.Round((decimal)(match?.Amount ?? 0), 2)
        //        };
        //    }).ToList();

        //    return result;
        //}

        //public IList<BarGraphDTO> GetLast30DaysSalesHistory(int supplierId)
        //{
        //    var toDate = DateTime.Now.Date;
        //    var fromDate = toDate.AddDays(-30);

        //    // Step 1: Perform group and sum in SQL
        //    var salesRaw = _dbContext.OrderDetails
        //        .Where(od => od.Order.Date >= fromDate
        //        && od.Order.Date <= toDate && !od.Order.IsDeleted
        //        && od.Product.SupplierId == supplierId)
        //        .GroupBy(od => od.Order.Date.Date) // Group by date only
        //        .Select(g => new
        //        {
        //            Date = g.Key,
        //            Amount = g.Sum(x => (x.Quantity - (x.ReturnQuantity ?? 0)) * x.UnitPrice)
        //        })
        //        .ToList(); // Bring data into memory

        //    // 🔁 Return early if no data
        //    if (salesRaw == null || !salesRaw.Any())
        //        return new List<BarGraphDTO>();

        //    // Step 2: Create all last 30 days
        //    var allDates = Enumerable.Range(0, 30)
        //        .Select(i => toDate.AddDays(-i))
        //        .OrderBy(d => d)
        //        .ToList();

        //    // Step 3: Merge raw data with date list
        //    var result = allDates.Select(d =>
        //    {
        //        var match = salesRaw.FirstOrDefault(x => x.Date.Date == d.Date);
        //        return new BarGraphDTO
        //        {
        //            MonthName = d.ToString("MMM dd"), // Show month and day
        //            Amount = Math.Round((decimal)(match?.Amount ?? 0), 2)
        //        };
        //    }).ToList();

        //    return result;
        //}

    }
}