using System.ComponentModel.DataAnnotations;

namespace DEALER.Domain
{
    public class StaffSalary
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public int PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        public int PaymentTypeId { get; set; }
        public virtual PaymentType PaymentType { get; set; }

        public DateTime SalaryDate { get; set; }

        [Required]
        public double Amount { get; set; }

        public string Description { get; set; }
        public bool IsDeleted { get; set; }


        // Navigation

        public virtual ICollection<TransactionHistory> TransactionHistories { get; set; }

        // NotMapped helpers
        public string EmployeeName => Employee?.Name;
        public string SalaryDateFormatted => SalaryDate.ToString("yyyy-MM-dd");
    }
}