namespace DEALER.DataAccess
{
    public interface IPurchase
    {
        bool CreatePurchase(Purchase purchase);
        bool UpdatePurchase(Purchase purchase);
        bool DeletePurchase(int id);
        Task<Purchase> GetPurchaseById(int purchaseId);
        Task<IEnumerable<PurchasesDetailsDTO>> GetPurchasesDetailsByOrderAsync(int purchaseId);
    }
}
