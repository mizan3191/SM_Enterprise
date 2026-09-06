namespace DEALER.DataAccess
{
    public interface IDSRShopPaymentHistory
    {
        Task<IList<DSRShopPaymentHistory>> GetAllDSRShopPaymentHistory(int shopId);

        Task<IList<DSRDueSummary>> GetTotalDuePerShop();
        Task<IList<ShopDuePaymentSummary>> GetShopDuePaymentSummary(int shopId);
        Task<IList<ShopDuePaymentListSummary>> GetAllShopDuePaymentListSummary();
        Task<IList<DSRShopPaymentHistory>> GetAllShopPaymentHistories();
        DSRShopPaymentHistory GetDSRShopPaymentHistory(int id);
        int CreateDSRShopPaymentHistory(DSRShopPaymentHistory DSRShopPaymentHistory);
        bool UpdateDSRShopPaymentHistory(DSRShopPaymentHistory DSRShopPaymentHistory);
        bool DeleteDSRShopPaymentHistory(int id);
    }
}
