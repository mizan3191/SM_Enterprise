namespace DEALER.DataAccess
{
    public interface IExpense
    {
        bool UpdateExpense(Expense expense);
        bool DeleteExpense(int id);
        int CreateExpense(Expense expense);
        Expense GetExpense(int id);
        Task<IList<Expense>> GetAllExpenses(DateTime? startDate, DateTime? endDate);
        Task<IList<Expense>> GetAllStaffSalary(DateTime? startDate, DateTime? endDate);
    }
}
