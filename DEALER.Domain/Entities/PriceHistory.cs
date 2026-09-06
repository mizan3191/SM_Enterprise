namespace DEALER.Domain
{
    public class PriceHistory
    {
        public int Id { get; set; } // Primary Key
        public double CylinderBuyingOldPrice { get; set; }
        public double CylinderBuyingNewPrice { get; set; }
        public double CylinderSellingOldPrice { get; set; }
        public double CylinderSellingNewPrice { get; set; }

        public double GasBuyingOldPrice { get; set; }
        public double GasBuyingNewPrice { get; set; }
        public double GasSellingOldPrice { get; set; }
        public double GasSellingNewPrice { get; set; }
        public DateTime Date { get; set; }
        public int ProductId { get; set; } // Foreign Key
        public virtual Product Product { get; set; } // Navigation Property
    }
}
