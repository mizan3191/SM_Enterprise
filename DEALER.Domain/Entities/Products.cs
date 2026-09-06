namespace DEALER.Domain
{
    public class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
            //CustomerProductReturnDetails = new HashSet<CustomerProductReturnDetails>();
            PurchaseDetails = new HashSet<PurchaseDetail>();
            ProductConsumptions = new HashSet<ProductConsumption>();
        }

        public int Id { get; set; }
        public int? ProductNo { get; set; }
        public string Name { get; set; }
        public int ReOrderLevel { get; set; }
        public int? Piece { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int? ProductsSizeId { get; set; }
        public ProductsSize ProductsSize { get; set; }

        public string DisplayText => $"{ProductNo} - {Name} - {ProductsSize?.Name}";
        public string DisplayNameSize => $"{Name} - {ProductsSize?.Name}";

        // Navigation to the split-out parts
        public virtual ProductPrice CurrentPrice { get; set; }
        public virtual ProductStock Stock { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // public virtual ICollection<CustomerProductReturnDetails> CustomerProductReturnDetails { get; set; }
        public virtual ICollection<PriceHistory> PriceHistories { get; set; }
        public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
        public virtual ICollection<ProductConsumption> ProductConsumptions { get; set; }
    }

    public class ProductPrice
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public double CylinderBuyingPrice { get; set; }
        public double CylinderSellingPrice { get; set; }
        public double GasBuyingPrice { get; set; }
        public double GasSellingPrice { get; set; }
    }

    public class ProductStock
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int LoadedQty { get; set; }
        public int EmptyQty { get; set; }

        public int TotalStockQty => LoadedQty + EmptyQty;

        public double LoadedStockValue(double cylinderBuyingPrice, double gasBuyingPrice)
            => LoadedQty * (cylinderBuyingPrice + gasBuyingPrice);

        public double EmptyStockValue(double cylinderBuyingPrice)
            => EmptyQty * cylinderBuyingPrice;
    }
}