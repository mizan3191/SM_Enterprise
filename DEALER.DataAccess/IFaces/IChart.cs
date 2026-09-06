using DEALER.Domain;

namespace DEALER.DataAccess
{
    public interface IChart
    {
        Task<IList<ProductSalesDTO>> LoadProductSales(int productId, DateTime? fromDate, DateTime? toDate);
        IList<BarGraphDTO> GetLast30DaysProfitHistory(DateTime? fromDate, DateTime? toDate); 
        IList<BarGraphDTO> Get30DaysSalesHistory(DateTime? fromDate, DateTime? toDate);
        IList<BarGraphDTO> Get30DaysDailyExpenseHistory(DateTime? fromDate, DateTime? toDate);
        IList<BarGraphDTO> GetLast12MonthExpenseHistory(DateTime? fromDate, DateTime? toDate);
        IList<BarGraphDTO> GetMonthlyProfitHistory(DateTime? fromDate, DateTime? toDate);
        IList<BarGraphDTO> GetLast12MonthSalesHistory(int supplierId);
        IList<BarGraphDTO> GetLast30DaysSalesHistory(int supplierId);

    }
}
