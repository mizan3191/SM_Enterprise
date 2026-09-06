namespace DEALER.DataAccess
{
    public interface IDailyExpense
    {
        int CreateDailyExpense(DailyExpense DailyExpense);
        bool UpdateDailyExpense(DailyExpense DailyExpense);

        int CreateDailyExpenseInOrder(DailyExpense DailyExpense);
        bool UpdateDailyExpenseInOrder(DailyExpense DailyExpense);
        bool DeleteDailyExpense(int id);
       

        DailyExpense GetDailyExpense(int id);
        Task<IList<DailyExpense>> GetAllDailyExpense(DateTime? startDate, DateTime? endDate);
        IList<DailyExpense> GetAllDailyExpense(int orderId);
    }
}
