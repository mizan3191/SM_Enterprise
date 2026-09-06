namespace DEALER.Domain
{
    public class PurchasesDetailsDTO
    {
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public int Quantity { get; set; }
        public int ReturnQuantity { get; set; }
        public double ProductPrice { get; set; }
        public double TotalPrice { get; set; }
        public double? Discount { get; set; }
    }
}

