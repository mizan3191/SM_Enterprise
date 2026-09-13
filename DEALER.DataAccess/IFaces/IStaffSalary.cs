namespace DEALER.DataAccess
{
    public interface IStaffSalary
    {
        int CreateStaffSalary(StaffSalary staffSalary);
        bool UpdateStaffSalary(StaffSalary staffSalary);
        StaffSalary GetStaffSalary(int id);
        Task<IList<StaffSalary>> GetAllStaffSalaries(DateTime? startDate, DateTime? endDate);
        bool DeleteStaffSalary(int id);
    }
}
