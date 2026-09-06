namespace DEALER.DataAccess
{
    public interface IOrderPaymentHistory
    {
        bool UpdateOrderPaymentHistory(OrderPaymentHistory OrderPaymentHistory);
        bool DeleteOrderPaymentHistory(int id);
        OrderPaymentHistory GetOrderPaymentHistoryById(int id);
        int CreateOrderPaymentHistory(OrderPaymentHistory OrderPaymentHistory);
        OrderPaymentHistory GetOrderPaymentHistory(int customerId, int orderId);
        Task<IList<OrderPaymentHistory>> GetAllOrderPaymentHistoryByOrderId(int orderId);
        Task<IList<OrderPaymentHistory>> GetAllOrderPaymentHistoryList();
    }
}