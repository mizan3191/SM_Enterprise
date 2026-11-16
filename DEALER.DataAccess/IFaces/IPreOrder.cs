namespace DEALER.DataAccess
{
    // ──────────────────────────────────────────────────────────────────────────
    // Delivery থেকে Purchase auto-create করার options
    // ──────────────────────────────────────────────────────────────────────────
    public class DeliveryPurchaseOptions
    {
        /// <summary>
        /// তৈরি হওয়া Purchase এর invoice নম্বর।
        /// না দিলে ChallanNumber ব্যবহার হবে।
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// তৈরি হওয়া Purchase এর Payment Method।
        /// </summary>
        public int? PaymentMethodId { get; set; }
        /// <summary>
        /// এই delivery তে এখন কত টাকা দেওয়া হচ্ছে।
        /// Pre-order এ আগেই টাকা দেওয়া থাকলে 0 রাখুন — due তে থাকবে।
        /// </summary>
        public double AmountPaid { get; set; } = 0;
        /// <summary>
        /// তৈরি হওয়া Purchase এর comments।
        /// </summary>
        public string Comments { get; set; }
    }
    // ──────────────────────────────────────────────────────────────────────────
    // IPreOrder — সম্পূর্ণ service contract
    // ──────────────────────────────────────────────────────────────────────────
    public interface IPreOrder
    {
        // ── CRUD ───────────────────────────────────────────────────────────────
        bool CreatePreOrder(PreOrder preOrder);
        bool UpdatePreOrder(PreOrder preOrder);
        bool DeletePreOrder(int id);
        // ── Detail helpers ─────────────────────────────────────────────────────
        /// <summary>
        /// কোম্পানি আর দেবে না এমন পরিমাণ বাতিল করো।
        /// </summary>
        bool CancelPendingQuantity(int preOrderDetailId, int quantity);
        // ── Payments ───────────────────────────────────────────────────────────
        bool AddPayment(PreOrderPayment payment);
        bool DeletePayment(int paymentId);
        // ── Deliveries ─────────────────────────────────────────────────────────
        /// <summary>
        /// সাধারণ delivery: stock + supplier ledger AddDeliveryInternal এ handle হয়।
        /// (পুরনো code এর সাথে backward compatible)
        /// </summary>
        bool AddDelivery(PreOrderDelivery delivery);
        /// <summary>
        /// Extended delivery: optionally একটা Purchase record তৈরি করে।
        /// createPurchase = true হলে Purchase auto-create হবে এবং
        /// stock ও ledger সেই Purchase-ই manage করবে।
        /// </summary>
        bool AddDeliveryWithPurchase(
            PreOrderDelivery delivery,
            bool createPurchase,
            DeliveryPurchaseOptions options = null);
        /// <summary>
        /// Delivery delete করো। Linked Purchase থাকলে সেটাও soft-delete হবে।
        /// </summary>
        bool DeleteDelivery(int deliveryId);
        // ── Queries ────────────────────────────────────────────────────────────
        Task<PreOrder> GetPreOrderById(int preOrderId);
        Task<IEnumerable<PreOrder>> GetAllPreOrders();
        /// <summary>
        /// Pending বা PartiallyDelivered PreOrder গুলো ফেরত দাও।
        /// </summary>
        Task<IEnumerable<PreOrder>> GetPendingPreOrders(int? supplierId = null);
    }
}