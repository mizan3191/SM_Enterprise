//namespace DEALER.Domain
//{
//    public class DamageProductReturn
//    {
//        public DamageProductReturn()
//        {
//            DamageProductReturnDetails = new HashSet<DamageProductReturnDetails>();
//            CustomerPaymentHistories = new HashSet<CustomerPaymentHistory>();
//        }

//        public int Id { get; set; }

//        public int CustomerId { get; set; }
//        public virtual Customer Customer { get; set; }

//        public int OrderId { get; set; }
//        public virtual Order Order { get; set; }


//        public double TotalAmount { get; set; }
//        public bool IsDeleted { get; set; }
//        public DateTime Date { get; set; } = DateTime.Now;
//        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

//        public virtual ICollection<DamageProductReturnDetails> DamageProductReturnDetails { get; set; }
//        public virtual ICollection<CustomerPaymentHistory> CustomerPaymentHistories { get; set; }

//    }
//}
