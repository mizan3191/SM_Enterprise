using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DEALER.Domain
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public string DateFormatted => ExpenseDate.ToString("dd-MMM-yyyy (ddd)");

        [Required]
        public int ExpenseTypeId { get; set; }
        public ExpenseType ExpenseType { get; set; } // e.g., Electricity Bill, Salary

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; } // e.g., Electricity Bill, Salary

        public string EmployeeName => Employee?.Name ?? string.Empty;

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public int PaymentMethodId { get; set; } // Cash, Bank, etc.
        public PaymentMethod PaymentMethod { get; set; } // Cash, Bank, etc.

        [StringLength(100)]
        public string ReferenceNumber { get; set; } // Bill or invoice number

        [StringLength(100)]
        public string PaidTo { get; set; } // e.g., staff, landlord, supplier

        public bool IsDeleted { get; set; } = false;

    }
}