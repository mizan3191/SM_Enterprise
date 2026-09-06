// DEALER.Domain/CustomerProductReturnDetails.cs
namespace DEALER.Domain
{
    public class CustomerProductReturnDetails
    {
        public int Id { get; set; }
        public int CustomerProductReturnId { get; set; }
        public virtual CustomerProductReturn CustomerProductReturn { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        // Original order details
        public int OrderQuantity { get; set; } // Quantity sold originally
        public int ReturnQuantity { get; set; } // Quantity being returned now

        // Unit prices from original sale
        public double CylinderUnitPrice { get; set; }
        public double GasUnitPrice { get; set; }

        // Return calculations
        public double CylinderReturnPrice { get; set; } // CylinderUnitPrice * ReturnQuantity
        public double GasReturnPrice { get; set; } // GasUnitPrice * ReturnQuantity

        public double Discount { get; set; } // Line item discount %
        public double ReturnPrice { get; set; } // Final price after discount
    }
}