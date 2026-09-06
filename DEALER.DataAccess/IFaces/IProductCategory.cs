namespace DEALER.DataAccess
{
    public interface IProductCategory
    {
        bool UpdateProductCategory(ProductCategory category);
        bool DeleteProductCategory(int id);
        int CreateProductCategory(ProductCategory category);
        ProductCategory GetProductCategory(int id);
        Task<IList<ProductCategory>> GetAllProductCategories();
        //Task<IList<Lov>> GetAllProductCategoryList();
    }
}
