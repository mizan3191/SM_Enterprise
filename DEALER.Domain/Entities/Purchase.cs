namespace DEALER.Domain
{
    public class Purchase
    {
        public Purchase()
        {
            PurchaseDetails = new HashSet<PurchaseDetail>();
            SupplierPaymentHistories = new HashSet<SupplierPaymentHistory>();
        }
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public string TransactionID { get; set; }
        public string Number { get; set; }
        public int? ShippingMethodId { get; set; }
        public virtual ShippingMethod ShippingMethod { get; set; }
        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
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
        // ──────────────────────────────────────────────────────────────────────
        // নতুন Field: Pre-Order integration
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Pre-Order Delivery থেকে auto-create হলে true।
        /// এই Purchase সরাসরি delete করা যাবে না।
        /// Pre-Order এর Delivery delete করলে এটা automatically soft-delete হবে।
        /// </summary>
        public bool IsFromPreOrder { get; set; } = false;
        /// <summary>
        /// কোন PreOrderDelivery থেকে তৈরি হয়েছে (traceability এর জন্য)।
        /// Delivery save হওয়ার পরে set হয়।
        /// </summary>
        public int? PreOrderDeliveryId { get; set; }
        public virtual PreOrderDelivery PreOrderDelivery { get; set; }
        // ──────────────────────────────────────────────────────────────────────
        public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; }
        public virtual ICollection<SupplierPaymentHistory> SupplierPaymentHistories { get; set; }
    }
}