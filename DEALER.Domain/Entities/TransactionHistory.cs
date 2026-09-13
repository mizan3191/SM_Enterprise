using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class TransactionHistory
    {
        public int Id { get; set; }
        public double? BalanceIn { get; set; }
        public double? BalanceOut { get; set; }
        public double? CurrentBalance { get; set; }
        public string Resone { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

        public bool IsDeleted { get; set; }

        [NotMapped]
        public double? Balance { get; set; }
        [NotMapped]
        public int BalanceTypeId { get; set; }



        public int? DailyExpenseId { get; set; }
        public DailyExpense DailyExpense { get; set; }

        public int? ExpenseId { get; set; }
        public Expense Expense { get; set; }

        public int? PurchaseId { get; set; }
        public Purchase Purchase { get; set; }

        public int? SupplierPaymentHistoryId { get; set; }
        public SupplierPaymentHistory SupplierPaymentHistory { get; set; }

       // public int? SRPaymentHistoryId { get; set; }
       // public SRPaymentHistory SRPaymentHistory { get; set; }

        public int? DSRShopPaymentHistoryId { get; set; }
        public DSRShopPaymentHistory DSRShopPaymentHistory { get; set; }

        public int? OrderId { get; set; }
        public Order Order { get; set; }

        public int? OrderPaymentHistoryId { get; set; }
        public OrderPaymentHistory OrderPaymentHistory { get; set; }

        public int? ProductConsumptionId { get; set; }
        public ProductConsumption ProductConsumption { get; set; }

        public int? CustomerPaymentHistoryId { get; set; }
        public CustomerPaymentHistory CustomerPaymentHistory { get; set; }

        public int? StaffSalaryId { get; set; }
        public StaffSalary StaffSalary { get; set; }
    }

    public enum BalanceTypeEnum
    {
        Debit = 1,
        Credit = 2
    }
}