namespace DEALER.Domain
{
    public class PurchaseDetailViewModel
    {

        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public int? ReturnQuantity { get; set; }
        public double Discount { get; set; }
        public double Price { get; set; } // Total price for the quantity purchased, after discount (Qty * UnitPrice - Discount)

        public double CylinderUnitPrice { get; set; } // Added for consistency // Value comes from the Product => ProductPrice => CylinderBuyingPrice
        public double GasUnitPrice { get; set; } // Added for consistency // Value comes from the Product => ProductPrice => GasBuyingPrice + CylinderBuyingPrice
    }
}
