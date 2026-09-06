namespace DEALER.Domain
{
    public class PreOrder
    {
        public PreOrder() 
        {
            PreOrderDetails = new HashSet<PreOrderDetails>();
        }

        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public double DeliveryCharge { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPay { get; set; }
        public double Discount { get; set; }
        public double TotalDue { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public virtual ICollection<PreOrderDetails> PreOrderDetails { get; set; }   
    }
}