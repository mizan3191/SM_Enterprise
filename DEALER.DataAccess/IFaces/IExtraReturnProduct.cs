namespace DEALER.DataAccess
{
    public interface IExtraReturnProduct
    {
        bool CreateExtraReturnProduct(ExtraReturnProduct ExtraReturnProduct);
        bool UpdateExtraReturnProduct(ExtraReturnProduct ExtraReturnProduct);
        Task<ExtraReturnProduct> GetExtraReturnProductByOrderId(int orderId);

       // Task<IEnumerable<ExtraReturnProductListDto>> GetListByOrderIdAsync(int orderId);

    }
}
