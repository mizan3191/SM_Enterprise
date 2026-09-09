namespace DEALER.Domain
{
    public class OrderDetailsDTO
    {
        public int ProductId { get; set; }  // New: Product ID যোগ করলাম
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int SellingQuantity { get; set; }
        public double ProductPrice { get; set; }  // Unit Price (Cylinder + Gas)
        public double TotalProductPrice { get; set; }  // Quantity * ProductPrice
        public double ReturnPrice { get; set; }  // ReturnQuantity * ProductPrice
        public double TotalPrice { get; set; }  // SellingQuantity * ProductPrice - Discount
        public double? Discount { get; set; }

        // New fields for detailed price breakdown
        public double CylinderUnitPrice { get; set; }  // Cylinder price per unit
        public double GasUnitPrice { get; set; }  // Gas price per unit
        public double TotalCylinderPrice { get; set; }  // Quantity * CylinderUnitPrice
        public double TotalGasPrice { get; set; }  // Quantity * GasUnitPrice
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
