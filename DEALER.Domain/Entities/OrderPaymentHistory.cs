using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class OrderPaymentHistory
    {
        public int Id { get; set; }
        public double AmountPaid { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        public int DSRCustomerId { get; set; }

        [ForeignKey(nameof(DSRCustomerId))]
        public virtual Customer DSRCustomer { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public bool IsDeleted { get; set; }
    }
}