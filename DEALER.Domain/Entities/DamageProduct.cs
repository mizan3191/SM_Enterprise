//namespace DEALER.Domain
//{
//    public class DamageProduct
//    {
//        public int Id { get; set; }

//        public int CustomerId { get; set; }
//        public virtual Customer Customer { get; set; }

//        public int? OrderId { get; set; }
//        public virtual Order Order { get; set; }

//        public int ProductId { get; set; }
//        public virtual Product Product { get; set; }

//        public double UnitPrice { get; set; }
//        public int Quantity { get; set; }
//        public bool IsReceived { get; set; }
//        public DateTime Date { get; set; }
//        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
//    }

//    public class DamageProductSummaryDTO
//    {
//        public string ProductName { get; set; }
//        public string SupplierName { get; set; }
//        public int TotalQuantity { get; set; }
//        public double TotalPrice { get; set; }
//    }

//    public class DamageStockSummaryDTO
//    {
//        public string SupplierName { get; set; }
//        public int SupplierId { get; set; }
//        public int StockQuantity { get; set; }
//        public double StockAmount { get; set; }
//    }

//}
