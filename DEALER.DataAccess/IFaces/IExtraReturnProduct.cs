namespace DEALER.DataAccess
{
    public interface IExtraReturnProduct
    {
        bool CreateExtraReturnProduct(ExtraReturnProduct ExtraReturnProduct);
        bool UpdateExtraReturnProduct(ExtraReturnProduct ExtraReturnProduct);
        Task<ExtraReturnProduct> GetExtraReturnProductByOrderId(int orderId);

        // Task<IEnumerable<ExtraReturnProductListDto>> GetListByOrderIdAsync(int orderId);

        // New method for detailed view
        Task<IEnumerable<ExtraReturnProductDetailsDto>> GetExtraReturnProductDetailsByOrderIdAsync(int orderId);

        // Company-wise grouped data
        Task<IEnumerable<ExtraReturnProductGroupDto>> GetExtraReturnProductGroupedByCompanyAsync(int orderId);
    }
}
