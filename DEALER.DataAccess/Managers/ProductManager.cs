using Microsoft.EntityFrameworkCore;

namespace DEALER.DataAccess
{
    public class ProductManager : BaseDataManager, IProduct
    {
        public ProductManager(DEALERContext model) : base(model)
        {
        }

      


        public Product GetProduct(int id)
        {
            try
            {
                return _dbContext.Products
                    .Include(c => c.ProductsSize)
                    .Include(c => c.CurrentPrice)
                    .Include(c => c.Supplier)
                    .FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ProductPrice GetProductPrice(int id)
        {
            try
            {
                return _dbContext.ProductPrices
                    .AsNoTracking()
                    .FirstOrDefault(c => c.ProductId == id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IList<Product>> GetAllProduct()
        {
            try
            {
                return await _dbContext.Products
                .Include(x => x.Supplier)
                .Include(x => x.ProductsSize)
                .Include(x => x.CurrentPrice)
                .Include(x => x.Stock)
                .OrderBy(x => x.Name)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Product>();
            }
        }

        //public async Task<IList<ProductDTO>> GetAllReOrderProducts()
        //{
        //    try
        //    {
        //        return await _dbContext.Products
        //            .Include(x => x.Supplier)
        //            .Include(x => x.ProductsSize)
        //            .Where(p => p.StockQty <= p.ReOrderLevel && !p.Supplier.IsDisable) // Filter products with low stock
        //            .Select(p => new ProductDTO
        //            {
        //                Id = p.Id,
        //                Name = p.DisplayNameSize,
        //                UnitOfMeasurementId = p.UnitOfMeasurementId ?? null,
        //                ReOrderLevel = p.ReOrderLevel,
        //                SupplierId = p.SupplierId,
        //                SupplierName = p.Supplier.Name,
        //                StockQty = p.StockQty
        //            })
        //            .OrderByDescending(c => c.Id)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<ProductDTO>(); // Ensure correct return type
        //    }
        //}

        public bool DeleteProduct(int id)
        {
            return RemoveEntity<Product>(id);
        }


        //public bool UpdateProductConsumption(ProductConsumption ProductConsumption)
        //{
        //    try
        //    {
        //        var entity = _dbContext.Products.FirstOrDefault(x => x.Id == ProductConsumption.ProductId);
        //        var Adjustment = _dbContext.ProductConsumptions.AsNoTracking().FirstOrDefault(x => x.Id == ProductConsumption.Id).QuantityConsumed;
        //        entity.StockQty = (entity.StockQty + Adjustment) - ProductConsumption.QuantityConsumed;

        //        AddUpdateEntity(ProductConsumption);
        //        _dbContext.SaveChanges();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //}

        //public int CreateProductConsumption(ProductConsumption ProductConsumption)
        //{
        //    try
        //    {
        //        _dbContext.ProductConsumptions.Add(ProductConsumption);
        //        _dbContext.SaveChanges();

        //        var entity = _dbContext.Products.FirstOrDefault(x => x.Id == ProductConsumption.ProductId);
        //        entity.StockQty = entity.StockQty - ProductConsumption.QuantityConsumed;
        //        _dbContext.SaveChanges();

        //        return ProductConsumption.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        return 0;
        //    }
        //}


        //public bool DeleteProductConsumption(int id)
        //{
        //    try
        //    {
        //        var productConsumption = _dbContext.ProductConsumptions.FirstOrDefault(x => x.Id == id);

        //        var entity = _dbContext.Products.FirstOrDefault(x => x.Id == productConsumption.ProductId);
        //        entity.StockQty = entity.StockQty + productConsumption.QuantityConsumed;

        //        _dbContext.Remove(productConsumption);
        //        _dbContext.SaveChanges();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //public ProductConsumption GetProductConsumption(int id)
        //{
        //    try
        //    {
        //        return _dbContext.ProductConsumptions.SingleOrDefault(c => c.Id == id);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}

        public bool UpdateProduct(Product updatedProduct)
        {
            if (updatedProduct == null || updatedProduct.Id <= 0)
            {
                return false;
            }

            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                // --- Product core fields update ---
                var existingProduct = _dbContext.Products
                    .Include(x => x.CurrentPrice)
                    .Include(x => x.Stock)
                    .FirstOrDefault(p => p.Id == updatedProduct.Id);

                if (existingProduct == null)
                    return false;

                // Product basic info update
                existingProduct.ProductNo = updatedProduct.ProductNo;
                existingProduct.Name = updatedProduct.Name;
                existingProduct.ReOrderLevel = updatedProduct.ReOrderLevel;
                existingProduct.Piece = updatedProduct.Piece;
                existingProduct.SupplierId = updatedProduct.SupplierId;
                existingProduct.ProductsSizeId = updatedProduct.ProductsSizeId;

                // --- Price update or create ---
                var newPrice = updatedProduct.CurrentPrice;

                if (existingProduct.CurrentPrice != null && newPrice != null)
                {
                    // পুরনো price snapshot (history এর জন্য)
                    var oldPrice = new ProductPrice
                    {
                        GasBuyingPrice = existingProduct.CurrentPrice.GasBuyingPrice,
                        GasSellingPrice = existingProduct.CurrentPrice.GasSellingPrice,
                        CylinderBuyingPrice = existingProduct.CurrentPrice.CylinderBuyingPrice,
                        CylinderSellingPrice = existingProduct.CurrentPrice.CylinderSellingPrice
                    };

                    // Update existing price
                    existingProduct.CurrentPrice.GasBuyingPrice = newPrice.GasBuyingPrice;
                    existingProduct.CurrentPrice.GasSellingPrice = newPrice.GasSellingPrice;
                    existingProduct.CurrentPrice.CylinderBuyingPrice = newPrice.CylinderBuyingPrice;
                    existingProduct.CurrentPrice.CylinderSellingPrice = newPrice.CylinderSellingPrice;

                    // Save changes before adding history
                    _dbContext.SaveChanges();

                    // --- Price history log (if price changed) ---
                    if (newPrice.GasBuyingPrice != oldPrice.GasBuyingPrice ||
                        newPrice.GasSellingPrice != oldPrice.GasSellingPrice ||
                        newPrice.CylinderBuyingPrice != oldPrice.CylinderBuyingPrice ||
                        newPrice.CylinderSellingPrice != oldPrice.CylinderSellingPrice)
                    {
                        _dbContext.PriceHistories.Add(new PriceHistory
                        {
                            ProductId = updatedProduct.Id,
                            GasBuyingOldPrice = oldPrice.GasBuyingPrice,
                            GasBuyingNewPrice = newPrice.GasBuyingPrice,
                            GasSellingOldPrice = oldPrice.GasSellingPrice,
                            GasSellingNewPrice = newPrice.GasSellingPrice,
                            CylinderBuyingOldPrice = oldPrice.CylinderBuyingPrice,
                            CylinderBuyingNewPrice = newPrice.CylinderBuyingPrice,
                            CylinderSellingOldPrice = oldPrice.CylinderSellingPrice,
                            CylinderSellingNewPrice = newPrice.CylinderSellingPrice,
                            Date = DateTime.UtcNow
                        });
                        _dbContext.SaveChanges();
                    }
                }
                else if (existingProduct.CurrentPrice == null && newPrice != null)
                {
                    // Create new price if doesn't exist
                    newPrice.ProductId = updatedProduct.Id;
                    _dbContext.ProductPrices.Add(newPrice);
                    _dbContext.SaveChanges();

                    // Add price history for new price
                    _dbContext.PriceHistories.Add(new PriceHistory
                    {
                        ProductId = updatedProduct.Id,
                        GasBuyingOldPrice = 0,
                        GasBuyingNewPrice = newPrice.GasBuyingPrice,
                        GasSellingOldPrice = 0,
                        GasSellingNewPrice = newPrice.GasSellingPrice,
                        CylinderBuyingOldPrice = 0,
                        CylinderBuyingNewPrice = newPrice.CylinderBuyingPrice,
                        CylinderSellingOldPrice = 0,
                        CylinderSellingNewPrice = newPrice.CylinderSellingPrice,
                        Date = DateTime.UtcNow
                    });
                    _dbContext.SaveChanges();
                }

                // --- Stock update or create ---
                var newStock = updatedProduct.Stock;

                if (existingProduct.Stock != null && newStock != null)
                {
                    // Update existing stock
                    existingProduct.Stock.LoadedQty = newStock.LoadedQty;
                    existingProduct.Stock.EmptyQty = newStock.EmptyQty;
                    _dbContext.SaveChanges();
                }
                else if (existingProduct.Stock == null && newStock != null)
                {
                    // Create new stock if doesn't exist
                    newStock.ProductId = updatedProduct.Id;
                    _dbContext.ProductStocks.Add(newStock);
                    _dbContext.SaveChanges();
                }

                transaction.Commit();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Concurrency error updating product: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error updating product: {ex.Message}");
                return false;
            }
        }


        public int CreateProduct(Product product)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                _dbContext.Products.Add(product);
                _dbContext.SaveChanges(); // এখন product.Id পাওয়া যাবে

                // ProductPrice আর ProductStock যদি cascade insert না হয়ে থাকে, নিশ্চিত করা
                if (product.CurrentPrice != null)
                {
                    product.CurrentPrice.ProductId = product.Id;
                    if (!_dbContext.ProductPrices.Local.Contains(product.CurrentPrice))
                        _dbContext.ProductPrices.Add(product.CurrentPrice);
                }

                if (product.Stock != null)
                {
                    product.Stock.ProductId = product.Id;
                    if (!_dbContext.ProductStocks.Local.Contains(product.Stock))
                        _dbContext.ProductStocks.Add(product.Stock);
                }

                _dbContext.SaveChanges();

                var price = product.CurrentPrice ?? new ProductPrice();

                _dbContext.PriceHistories.Add(new PriceHistory
                {
                    ProductId = product.Id,
                    GasBuyingOldPrice = price.GasBuyingPrice,
                    GasBuyingNewPrice = price.GasBuyingPrice,
                    GasSellingOldPrice = price.GasSellingPrice,
                    GasSellingNewPrice = price.GasSellingPrice,
                    CylinderBuyingOldPrice = price.CylinderBuyingPrice,
                    CylinderBuyingNewPrice = price.CylinderBuyingPrice,
                    CylinderSellingOldPrice = price.CylinderSellingPrice,
                    CylinderSellingNewPrice = price.CylinderSellingPrice,
                    Date = DateTime.UtcNow
                });

                _dbContext.SaveChanges();

                transaction.Commit();
                return product.Id;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Concurrency error added product: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error added product: {ex.Message}");
                return 0;
            }
        }

        public async Task<IList<ProductConsumptionDTO>> GetAllProductConsumption(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Default to last 3 days if no date is provided
                DateTime fromDate = startDate.HasValue
                    ? new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0)
                    : DateTime.Today.AddDays(-30); // default to last 7 days

                DateTime toDate = endDate.HasValue
                    ? new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59)
                    : DateTime.Today.AddDays(1).AddSeconds(-1); // today till 23:59:59



                return await _dbContext.ProductConsumptions
                    .Include(x => x.Product)
                    .ThenInclude(x => x.ProductsSize)
                   //.Include(x => x.Customer)
                    .Include(x => x.ReasonofAdjustment)
                    .Where(x => !x.IsDeleted && x.DateConsumed.Date >= fromDate && x.DateConsumed.Date <= toDate)
                    .Select(x => new ProductConsumptionDTO
                    {
                        Id = x.Id,
                        QuantityConsumed = x.QuantityConsumed,
                        ReasonOfConsumed = x.ReasonofAdjustment.Name,
                        DateConsumed = x.DateConsumed,
                       // Person = x.Customer != null ? x.Customer.Name : string.Empty,
                        ProductName = x.Product != null ? x.Product.DisplayNameSize : string.Empty
                    })
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<ProductConsumptionDTO>();
            }
        }

        public async Task<IList<ProductDTO>> GetAllReOrderProducts()
        {
            var products = await _dbContext.Products
                .Include(p => p.Supplier)
                .Include(p => p.Stock)
                .Where(p => p.Stock != null && p.Stock.LoadedQty <= p.ReOrderLevel)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.Name,
                    Name = p.Name,
                    ReOrderLevel = p.ReOrderLevel,
                    StockQty = p.Stock.LoadedQty
                })
                .OrderBy(p => p.StockQty)
                .ToListAsync();

            return products;
        }

        public bool UpdateProductConsumption(ProductConsumption ProductConsumption)
        {
            throw new NotImplementedException();
        }

        public bool DeleteProductConsumption(int id)
        {
            throw new NotImplementedException();
        }

        public int CreateProductConsumption(ProductConsumption ProductConsumption)
        {
            throw new NotImplementedException();
        }

        public ProductConsumption GetProductConsumption(int id)
        {
            throw new NotImplementedException();
        }
    }
}