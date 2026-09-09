namespace DEALER.Domain
{
    public class CustomerPaymentHistory
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? CustomerProductReturnId { get; set; }
        public virtual CustomerProductReturn CustomerProductReturn { get; set; }

        // public int? DamageProductReturnId { get; set; }
        // public virtual DamageProductReturn DamageProductReturn { get; set; }

        public int? ExtraReturnProductId { get; set; }
        public virtual ExtraReturnProduct ExtraReturnProduct { get; set; }

        public int? DSRShopDueId { get; set; }
        public virtual DSRShopDue DSRShopDue { get; set; }

        public int? DailyExpenseId { get; set; }
        public virtual DailyExpense DailyExpense { get; set; }

        //  public int? SRDiscountId { get; set; }
        // public virtual SRDiscount SRDiscount { get; set; }

        public int? OrderPaymentHistoryId { get; set; }
        public virtual OrderPaymentHistory OrderPaymentHistory { get; set; }

        public double? TotalAmountThisOrder { get; set; }  // Amount paid in this transaction
        public double AmountPaid { get; set; }  // Amount paid in this transaction
        public double TotalDueBeforePayment { get; set; } // Due before this payment
        public double TotalDueAfterPayment { get; set; }  // Due after this payment
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string DateFormatted => PaymentDate.ToString("dd-MMM-yyyy (ddd)");

        public int? PaymentMethodId { get; set; }  // Cash, Card, etc.
        public PaymentMethod PaymentMethod { get; set; }  // Cash, Card, etc.

        public string TransactionID { get; set; }
        public string Number { get; set; }

        public bool IsDeleted { get; set; }
    }
}