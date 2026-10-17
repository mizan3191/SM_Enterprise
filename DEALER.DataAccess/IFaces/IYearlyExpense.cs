namespace DEALER.DataAccess
{
    public interface IYearlyExpense
    {
        int CreateYearlyExpense(YearlyExpense expense);
        bool UpdateYearlyExpense(YearlyExpense expense);
        bool DeleteYearlyExpense(int id);
        YearlyExpense GetYearlyExpense(int id);
        Task<IList<YearlyExpense>> GetAllYearlyExpenses(DateTime? startDate, DateTime? endDate);
    }

}
