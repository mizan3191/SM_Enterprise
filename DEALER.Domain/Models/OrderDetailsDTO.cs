namespace DEALER.Domain
{
    public class OrderDetailsDTO
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int SellingQuantity { get; set; }
        public double ProductPrice { get; set; }
        public double TotalProductPrice { get; set; }
        public double ReturnPrice { get; set; }
        public double TotalPrice { get; set; }
        public double? Discount { get; set; }
    }

    public class DamageProductDetailsDTO
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double ProductPrice { get; set; }
        public double TotalPrice { get; set; }
    }

    public class OrderExportToPdfDTO
    {
        public string ProductName { get; set; }
        public int S_CB { get; set; }
        public int S_PD { get; set; }
        public int S_PQ { get; set; }
        public int R_CB { get; set; }
        public int R_PD { get; set; }
        public int R_PQ { get; set; }
    }
   
}
