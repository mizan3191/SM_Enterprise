namespace DEALER.DataAccess
{
    public interface ICylinderExchange
    {
        CylinderExchange GetCylinderExchangeById(int id);
        IEnumerable<CylinderExchange> GetAllCylinderExchange();
        CylinderExchange GetCylinderExchangeBySupplierId(int supplierId);
        void CreateCylinderExchange(CylinderExchange exchange);
        void UpdateCylinderExchange(CylinderExchange exchange);
        void DeleteCylinderExchange(int id);

        Task<IEnumerable<CylinderExchangeDetailsDTO>> GetCylinderExchangeDetailsByIdAsync(int exchangeId);
    }
}
