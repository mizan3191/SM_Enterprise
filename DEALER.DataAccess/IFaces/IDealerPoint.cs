namespace DEALER.DataAccess
{
    public interface IDealerPoint
    {
        Task<List<DealerPoint>> GetAllDealerPoint(bool includeDisabled = true);
        Task<DealerPoint?> GetDealerPointById(int id);
        Task<bool> IsNameExists(string name, int? excludeId = null);
        int CreateDealerPoint(DealerPoint dealerPoint); 
        bool UpdateDealerPoint(DealerPoint dealerPoint);
        Task<bool> SetDisabled(int id, bool isDisable);
    }
}
