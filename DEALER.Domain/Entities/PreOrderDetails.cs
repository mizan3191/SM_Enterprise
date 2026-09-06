namespace DEALER.Domain
{
    public class PreOrderDetails
    {
        public int Id { get; set; }
        public int PreOrderId { get; set; }
        public virtual PreOrder PreOrder { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
    }
}