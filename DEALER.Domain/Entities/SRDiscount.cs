//using System.ComponentModel.DataAnnotations.Schema;

//namespace DEALER.Domain
//{
//    public class SRDiscount
//    {
//        public int Id { get; set; }

//        [ForeignKey("Customer")]
//        public int CustomerId { get; set; }
//        public virtual Customer Customer { get; set; }

//        [ForeignKey("DSRCustomer")]
//        public int DSRCustomerId { get; set; }
//        public virtual Customer DSRCustomer { get; set; }

//        public int OrderId { get; set; }
//        public virtual Order Order { get; set; }

//        public double DiscountAmount { get; set; }
//        public bool IsDeleted { get; set; }
//        public DateTime Date { get; set; } = DateTime.Now;
//        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
//    }
//}
