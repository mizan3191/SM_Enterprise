namespace DEALER.DataAccess
{
    public interface IProductReturn
    {
        bool CreateCustomerProductReturn(CustomerProductReturn customerProductReturn);
        bool UpdateCustomerProductReturn(CustomerProductReturn customerProductReturn);
        bool DeleteCustomerProductReturn(int customerProductReturnId);
        Task<CustomerProductReturn> GetCustomerProductReturnByOrderId(int orderId);
        Task<CustomerProductReturn> GetCustomerProductReturnById(int Id);
        Task<CustomerProductReturn> GetExistingCustomerProductReturnByOrderId(int orderId);
        Task<IEnumerable<CustomerProductReturnDTO>> GetAllCustomerReturnProducts();


    }
}
