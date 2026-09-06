namespace DEALER.Domain
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public double Discount { get; set; }

        public int Quantity { get; set; }
        public int? ReturnQuantity { get; set; }

        public double CylinderUnitPrice { get; set; }
        public double GasUnitPrice { get; set; }

        public double Price { get; set; }
    }
}