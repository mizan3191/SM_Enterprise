namespace DEALER.Domain
{
    public class Purchase
    {
        public Purchase()
        {
            PurchaseDetails = new HashSet<PurchaseDetail>();
            SupplierPaymentHistories = new HashSet<SupplierPaymentHistory>();
        }

        public int Id { get; set; } // Primary Key

        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        public string TransactionID { get; set; }
        public string Number { get; set; }

        public int? ShippingMethodId { get; set; }
        public ShippingMethod ShippingMethod { get; set; }

        public int? PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public double DeliveryCharge { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPay { get; set; }
        public double Discount { get; set; }
        public double DamageProductDueAdjustment { get; set; }
        public double Adjustment { get; set; }
        public double TotalDue { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; }
        public string Comments { get; set; }

        public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
        public virtual ICollection<SupplierPaymentHistory> SupplierPaymentHistories { get; set; }

    }    
}