namespace DEALER.Domain
{
    public class SupplierPaymentHistory
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public int? PurchaseId { get; set; }
        public virtual Purchase Purchase { get; set; }

        // Pre-order e deya payment
        public int? PreOrderPaymentId { get; set; }
        public virtual PreOrderPayment PreOrderPayment { get; set; }

        // Pre-order er delivery (product paoa)
        public int? PreOrderDeliveryId { get; set; }
        public virtual PreOrderDelivery PreOrderDelivery { get; set; }

        public double? TotalAmountThisPurchase { get; set; }
        public double AmountPaid { get; set; }
        public double TotalDueBeforePayment { get; set; }
        public double TotalDueAfterPayment { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string DateFormatted => PaymentDate.ToString("dd-MMM-yyyy (ddd)");

        public int? PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public string TransactionID { get; set; }
        public string Number { get; set; }
        public bool IsDeleted { get; set; }
        public string Comments { get; set; }
    }
}