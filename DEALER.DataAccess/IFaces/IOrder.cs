namespace DEALER.DataAccess
{
    public interface IOrder
    {
        bool CreateOrder(Order order);
        bool UpdateOrder(Order order);
        bool DeleteOrder(int id);
        Task<IEnumerable<OrdersDTO>> GetAllOrders(DateTime? startDate, DateTime? endDate);
        //IList<OrdersDTO> GetAllOrderss();
        Task<Order> GetOrderById(int orderId);
        Task<OrderInfo> GetOrderInfoById(int orderId);
        PersonInfoDTO GetPersonInfo(int personId);
        IEnumerable<OrdersByPersonDTO> GetAllOrdersByPerson(int personId);
        IEnumerable<OrderDetailsDTO> GetOrderDetailsByOrderId(int orderId);
        Task<IEnumerable<OrderDetailsDTO>> GetOrderDetailsByOrderAsync(int orderId);
        Task<IEnumerable<ExistingOrderDTO>> GetExistingOrderById(int orderId);
        //Task<IEnumerable<DamageProductDetailsDTO>> GetDamageProductDetailsByOrderAsync(int orderId);
        IEnumerable<CustomerDueDTO> GetCustomerDueHistory();
        InvoiceDTO GetOrdersById(int id);
        OrderInfoDTO OrderInfoById(int id);
        List<(string name, double value)> GetPayment(int id);
        Task<IList<CustomerPaymentHistoryDTO>> GetCustomerPaymentHistoryById(int customerId);

        CustomerPaymentHistoryDTO DuePayment(int id);
    }
}
