namespace DEALER.DataAccess
{
    public interface IPreOrder
    {
        // ---------- অর্ডার ----------
        // নতুন অর্ডার তৈরি (preOrder.TotalPaid > 0 হলে প্রথম পেমেন্ট নিজে থেকেই তৈরি হবে)
        bool CreatePreOrder(PreOrder preOrder);

        // অর্ডার আপডেট (ডেলিভারি শুরু হয়ে গেলে শুধু তারিখ/মন্তব্য বদলানো যাবে)
        bool UpdatePreOrder(PreOrder preOrder);

        // অর্ডার ডিলিট (ডেলিভারি থাকলে ডিলিট হবে না)
        bool DeletePreOrder(int id);

        // কোম্পানি যে পিস আর দেবে না, সেটা বাতিল করা
        bool CancelPendingQuantity(int preOrderDetailId, int quantity);

        // ---------- পেমেন্ট ----------
        bool AddPayment(PreOrderPayment payment);
        bool DeletePayment(int paymentId);

        // ---------- ডেলিভারি ----------
        bool AddDelivery(PreOrderDelivery delivery);
        bool DeleteDelivery(int deliveryId);

        // ---------- রিড ----------
        Task<PreOrder> GetPreOrderById(int preOrderId);
        Task<IEnumerable<PreOrder>> GetAllPreOrders();

        // যে অর্ডারের পণ্য এখনো বাকি (Pending / PartiallyDelivered)
        Task<IEnumerable<PreOrder>> GetPendingPreOrders(int? supplierId = null);
    }
}