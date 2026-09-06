using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class OrderPaymentHistory
    {
        public int Id { get; set; }
        public double AmountPaid { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

        public int? EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee Employee { get; set; }

        public int DSREmployeeId { get; set; }

        [ForeignKey(nameof(DSREmployeeId))]
        public virtual Employee DSREmployee { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public bool IsDeleted { get; set; }
    }
}