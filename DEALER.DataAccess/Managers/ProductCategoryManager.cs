namespace DEALER.DataAccess
{
    public class ProductCategoryManager : BaseDataManager, IProductCategory
    {
        public ProductCategoryManager(DEALERContext model) : base(model)
        {
        }

        public int CreateProductCategory(ProductCategory category)
        {
            throw new NotImplementedException();
        }

        public bool DeleteProductCategory(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ProductCategory>> GetAllProductCategories()
        {
            throw new NotImplementedException();
        }

        public ProductCategory GetProductCategory(int id)
        {
            throw new NotImplementedException();
        }

        public bool UpdateProductCategory(ProductCategory category)
        {
            throw new NotImplementedException();
        }

        //public bool UpdateProductCategory(ProductCategory category)
        //{
        //    return AddUpdateEntity(category);
        //}

        //public int CreateProductCategory(ProductCategory category)
        //{
        //    _dbContext.ProductCategories.Add(category);
        //    _dbContext.SaveChanges();

        //    // Now update ProductNo based on the ID
        //    category.ProductNo = category.Id * 1000;    

        //    // Save the updated ProductNo
        //    _dbContext.ProductCategories.Update(category);
        //    _dbContext.SaveChangesAsync();

        //    return category.Id;
        //}

        //public ProductCategory GetProductCategory(int id)
        //{
        //    try
        //    {
        //        return _dbContext.ProductCategories.SingleOrDefault(c => c.Id == id);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        //public async Task<IList<ProductCategory>> GetAllProductCategories()
        //{
        //    try
        //    {
        //        return await _dbContext.ProductCategories
        //            .OrderBy(x => x.Name)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<ProductCategory>();
        //    }
        //}

        //public bool DeleteProductCategory(int id)
        //{
        //    try
        //    {
        //        var IsExistCategory = _dbContext.ProductCategories.SingleOrDefault(c => c.Id == id);

        //        if (IsExistCategory is null)
        //        {
        //            return false; // Cannot delete category if it has associated products
        //        }

        //        var isDelete = _dbContext.Products.Any(c => c.ProductCategoryId == id);
        //        if (!isDelete)
        //        {
        //            _dbContext.Remove(IsExistCategory);
        //            _dbContext.SaveChanges();
        //        }
        //        else
        //        {
        //            // If no customers are associated, proceed with deletion
        //            IsExistCategory.IsDeleted = true;
        //            _dbContext.Update(IsExistCategory);
        //            _dbContext.SaveChanges();
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
    }
}
