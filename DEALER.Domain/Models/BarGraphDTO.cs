namespace DEALER.Domain
{
    public class BarGraphDTO
    {
        public string MonthName { get; set; }
        public decimal Amount { get; set; }
    }

    public class LastMonthHistoryGraphDTO
    {
        public int TotalPurchase { get; set; }
        public double TotalPurchasePrice { get; set; }
        public double TotalProfit { get; set; }
        public int TotalSales { get; set; }
        public double TotalSalesPrice { get; set; }
        public int TotalConsume { get; set; }
    }
}