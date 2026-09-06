namespace DEALER.Domain
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Name { get; set; }
        public int ReOrderLevel { get; set; }
        public int StockQty { get; set; }
    }
}