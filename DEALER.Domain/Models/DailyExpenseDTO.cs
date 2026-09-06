using System.ComponentModel.DataAnnotations;

namespace DEALER.Domain
{
    public class DailyExpenseDTO
    {
        public int Id { get; set; }

        public string DailyExpenseType { get; set; }
        public string CustomerName { get; set; }
        public int? OrderId { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
    }
}