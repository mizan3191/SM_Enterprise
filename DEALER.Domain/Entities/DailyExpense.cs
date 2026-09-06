using System.ComponentModel.DataAnnotations;

namespace DEALER.Domain
{
    public class DailyExpense
    {
        public int Id { get; set; }

        public int DailyExpenseTypeId { get; set; }
        public DailyExpenseType DailyExpenseType { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? OrderId { get; set; }
        public Order Order { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
        public bool IsDeleted { get; set; } = false;
    }
}