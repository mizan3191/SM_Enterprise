namespace DEALER.Domain
{
    public class PurchaseDetail
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }
        public virtual Purchase Purchase { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }
        public int? ReturnQuantity { get; set; }


        public double? Discount { get; set; }
        public double Price { get; set; } // Total price for the quantity purchased, after discount (Qty * UnitPrice - Discount)

        public double CylinderUnitPrice { get; set; } // Added for consistency // Value comes from the Product => ProductPrice => CylinderBuyingPrice
        public double GasUnitPrice { get; set; } // Added for consistency // Value comes from the Product => ProductPrice => GasBuyingPrice
    }
}