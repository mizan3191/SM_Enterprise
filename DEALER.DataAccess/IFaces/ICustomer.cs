namespace DEALER.DataAccess
{
    public interface ICustomer
    {
        Task<IList<Lov>> GetAllClients();
        int AddCustomerPayment(CustomerPaymentHistory customerPayment);
        bool UpdateCustomerPayment(CustomerPaymentHistory customerPayment);
        CustomerPaymentHistory GetCustomerPaymentHistory(int id);
        bool DeleteCustomerPayment(int id);


        #region Customer
        bool UpdateCustomer(Customer customer);
        bool DeleteCustomer(int id);
        int CreateCustomer(Customer customer);
        Customer GetCustomer(int id);
        string GetCustomerName(int id);
        Task<IList<Customer>> GetAllCustomer();

        #endregion Customer


        #region Employee
        bool UpdateEmployee(Employee employee);
        bool DeleteEmployee(int id); 
        int CreateEmployee(Employee employee);
        Employee GetEmployee(int id);
        string GetEmployeeName(int id);
        Task<IList<Employee>> GetAllEmployees();

        #endregion Employee
    }
}