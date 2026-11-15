namespace DEALER.DataAccess
{
    public class DealerPointManager : BaseDataManager, IDealerPoint
    {
        public DealerPointManager(DEALERContext model) : base(model)
        {
        }

        public async Task<List<DealerPoint>> GetAllDealerPoint(bool includeDisabled = true)
        {
            var query = _dbContext.DealerPoints.AsNoTracking().AsQueryable();

            if (!includeDisabled)
                query = query.Where(d => !d.IsDisable);

            return await query.OrderBy(d => d.Name).ToListAsync();
        }

        public async Task<DealerPoint?> GetDealerPointById(int id)
        {
            return await _dbContext.DealerPoints
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> IsNameExists(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();

            return await _dbContext.DealerPoints
                .AsNoTracking()
                .AnyAsync(d => d.Name.ToLower() == normalized
                               && (excludeId == null || d.Id != excludeId.Value));
        }

        public int CreateDealerPoint(DealerPoint dealerPoint)
        {
            AddUpdateEntity(dealerPoint);
            return dealerPoint.Id;
        }

        public bool UpdateDealerPoint(DealerPoint dealerPoint)
        {
            return AddUpdateEntity(dealerPoint);
        }

        public async Task<bool> SetDisabled(int id, bool isDisable)
        {
            var existing = await _dbContext.DealerPoints
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existing == null)
                return false;

            existing.IsDisable = isDisable;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
