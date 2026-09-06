// DEALER.Domain/ReturnOrderDetailViewModel.cs
namespace DEALER.Domain
{
    public class ReturnOrderDetailViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Product { get; set; }

        public int Quantity { get; set; } // Original sold quantity
        public int ReturnQuantity { get; set; } // Quantity to return

        public double CylinderUnitPrice { get; set; }
        public double GasUnitPrice { get; set; }

        public double CylinderReturnPrice { get; set; }
        public double GasReturnPrice { get; set; }

        public double Discount { get; set; }
        public double ReturnPrice { get; set; } // Total return price
    }
}