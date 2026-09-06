using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DEALER.Domain
{
    public class SupplierPaymentHistory
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int? PurchaseId { get; set; }
        public virtual Purchase Purchase { get; set; }

        public double? TotalAmountThisPurchase { get; set; }  // Amount paid in this transaction
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
        public string Comments { get; set; }
    }
}