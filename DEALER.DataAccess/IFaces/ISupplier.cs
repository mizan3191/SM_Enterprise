namespace DEALER.DataAccess
{
    public interface ISupplier
    {
        bool UpdateSupplier(Supplier supplier);
        int CreateSupplier(Supplier supplier);
        bool DeleteSupplier(int id);
        Supplier GetSupplier(int id);
        Task<IList<Supplier>> GetAllSupplier();
        

        Task<IEnumerable<PurchaseDTO>> GetAllPurchases(DateTime? startDate, DateTime? endDate);
        Task<IList<SupplierPaymentHistoryDTO>> GetSupplierPaymentHistoryById(int supplierId);
        

        IEnumerable<SupplierDueDTO> GetSupplierDueHistory();
        IEnumerable<OrdersByPersonDTO> GetAllPurchasesByPerson(int personId);

        int AddSupplierPayment(SupplierPaymentHistory supplierPayment);
        bool UpdateSupplierPayment(SupplierPaymentHistory supplierPayment);
        SupplierPaymentHistory GetSupplierPaymentHistory(int id);
        bool DeleteSupplierPayment(int id);

    }
}