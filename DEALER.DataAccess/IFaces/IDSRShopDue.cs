namespace DEALER.DataAccess
{
    public interface IDSRShopDue
    {
        int CreateDSRShopDue(DSRShopDue DSRShopDue);
        bool UpdateDSRShopDue(DSRShopDue DSRShopDue);
        bool DeleteShopDue(int id);

        bool AddShopEmptyCylinderProduct(ShopEmptyCylinderProduct product);
        bool DeleteShopEmptyCylinderProduct(int productId);


        DSRShopDue GetDSRShopDue(int id);
        IList<DSRShopDue> GetAllDSRShopDue();
        Task<IList<Lov>> GetAllShopDueCustomerList(int shopId);
        IList<DSRShopDueDTO> GetAllDSRShopDueList(DateTime? startDate, DateTime? endDate);
        IList<DSRShopDueDTO> GetAllDSRShopDueList(int shopId, DateTime? startDate, DateTime? endDate);
        Task<IList<DSRShopDue>> GetAllDSRShopDue(int orderId);
        Task<double> LoadShopDueCustomerWise(int shopId, int customerId);

        //int CreateDSRShopDueInOrderTime(DSRShopDue DSRShopDue);
        //bool UpdateDSRShopDueInOrderTime(DSRShopDue DSRShopDue);
        //bool DeleteShopDueInOrderTime(int id);
        IList<DSRShopDueForOrderDTO> GetAllDSRShopDueByOrderId(int orderId);

        // New methods
        Task<IEnumerable<ShopDueWithCylinderDto>> GetShopDueWithCylindersAsync(int orderId);
        Task<IEnumerable<ShopCylinderGroupDto>> GetShopCylindersGroupedBySupplierAsync(int orderId);
    }
}
