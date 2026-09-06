//namespace DEALER.DataAccess
//{
//    public interface ISRDiscount
//    {
//        bool UpdateSRDiscount(SRDiscount SRDiscount);
//        bool DeleteSRDiscount(int id);
//        int CreateSRDiscount(SRDiscount SRDiscount);
//        SRDiscount GetSRDiscount(int customerId, int orderId);
//        SRDiscount GetSRDiscountById(int id);
//        SRDiscount GetSRDiscountByOrderId(int orderId);
//        IList<SRDiscountDTO> GetAllSRDiscount_ByOrderId(int orderId);
//        Task<IList<SRDiscount>> GetAllSRDiscountByOrderId(int orderId);
//        Task<IList<SRDiscount>> GetAllSRDiscountList(int customerId, DateTime? startDate, DateTime? endDate);
//        Task<IList<SRDiscount>> GetAllSRDiscount(int customerId);
//        Task<IList<SRDiscountSummary>> GetTotalDiscountPerSRAsync();
//        Task<IList<SRDiscountDuePaymentSummary>> GetSRDiscountDuePaymentSummary(int customerId);
//    }
//}
