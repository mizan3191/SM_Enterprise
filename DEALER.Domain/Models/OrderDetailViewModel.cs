namespace DEALER.Domain
{
    public class OrderDetailViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public int? ReturnQuantity { get; set; }
        public double Discount { get; set; }
        public double Price { get; set; }

        public double CylinderUnitPrice { get; set; }
        public double GasUnitPrice { get; set; }
    }   
}