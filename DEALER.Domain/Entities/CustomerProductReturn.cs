// DEALER.Domain/CustomerProductReturn.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class CustomerProductReturn
    {
        public CustomerProductReturn()
        {
            CustomerProductReturnDetails = new HashSet<CustomerProductReturnDetails>();
            CustomerPaymentHistories = new HashSet<CustomerPaymentHistory>();
        }

        public int Id { get; set; }

        public int? EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        public double TotalAmount { get; set; } // Total return amount
        public double TotalReturnPrice { get; set; } // Total return price after discount
        public double Discount { get; set; } // Overall discount on return

        public DateTime Date { get; set; } = DateTime.Now;
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
        public bool IsDeleted { get; set; }
        public string Remarks { get; set; }

        public virtual ICollection<CustomerProductReturnDetails> CustomerProductReturnDetails { get; set; }
        public virtual ICollection<CustomerPaymentHistory> CustomerPaymentHistories { get; set; }
    }
}