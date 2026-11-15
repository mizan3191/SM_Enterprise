namespace DEALER.Domain
{
    // =====================================================================
    // PRE-ORDER (আগাম অর্ডার)
    //
    // কীভাবে কাজ করে:
    //   ১. কোম্পানিকে আগে অর্ডার দেওয়া হয় (PreOrder + PreOrderDetail)
    //   ২. অর্ডারের টাকা (পুরো বা আংশিক) আগেই পরিশোধ করা হয় (PreOrderPayment)
    //   ৩. কয়েকদিন পর কোম্পানি পণ্য দেয় (PreOrderDelivery + PreOrderDeliveryDetail)
    //   ৪. কোম্পানি একবারে পুরো পণ্য না-ও দিতে পারে, কয়েকবারে দিতে পারে
    //
    // উদাহরণ:
    //   ১০০০ পিস অর্ডার, ১০০০ পিসের টাকাই পরিশোধ
    //   প্রথম ডেলিভারি: ৯০০ পিস  => বাকি (Pending) ১০০ পিস
    //   পরের ডেলিভারি: ১০০ পিস   => বাকি (Pending) ০, Status = Delivered
    //
    // এখানে টাকা দেওয়া আর পণ্য পাওয়া আলাদা হিসাব রাখা হয়েছে,
    // তাই "টাকা দিয়েছি কিন্তু পণ্য পাইনি" এটা সহজে বের করা যাবে।
    // =====================================================================

    /// <summary>
    /// অর্ডারের বর্তমান অবস্থা
    /// </summary>
    public enum PreOrderStatus
    {
        Pending = 0,              // অর্ডার দেওয়া হয়েছে, এখনো কোনো পণ্য আসেনি
        PartiallyDelivered = 1,   // কিছু পণ্য এসেছে, কিছু বাকি আছে
        Delivered = 2,            // সব পণ্য পাওয়া গেছে
        Cancelled = 3             // অর্ডার বাতিল
    }

    /// <summary>
    /// আগাম অর্ডারের মূল তথ্য (Header)
    /// </summary>
    public class PreOrder
    {
        public PreOrder()
        {
            PreOrderDetails = new HashSet<PreOrderDetail>();
            PreOrderPayments = new HashSet<PreOrderPayment>();
            PreOrderDeliveries = new HashSet<PreOrderDelivery>();
        }

        public int Id { get; set; }

        // কোন কোম্পানি/সাপ্লায়ারকে অর্ডার দেওয়া হয়েছে
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        // অর্ডার নম্বর (যেমন: PO-2026-0001)
        public string OrderNumber { get; set; }

        // অর্ডার দেওয়ার তারিখ
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // কোম্পানি কবে নাগাদ পণ্য দেবে বলেছে (জানা না থাকলে খালি থাকবে)
        public DateTime? ExpectedDeliveryDate { get; set; }

        // অর্ডারের অবস্থা: Pending / PartiallyDelivered / Delivered / Cancelled
        public PreOrderStatus Status { get; set; } = PreOrderStatus.Pending;

        // অর্ডারের সময় যে পেমেন্ট মেথড ব্যবহার করা হয়েছে
        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        // ডিসকাউন্টের আগের মোট দাম (সব আইটেমের যোগফল)
        public double SubTotal { get; set; }

        // পুরো অর্ডারের উপর ডিসকাউন্ট (টাকা)
        public double Discount { get; set; }

        // চূড়ান্ত মোট দাম = SubTotal - Discount
        public double TotalAmount { get; set; }

        // এখন পর্যন্ত কোম্পানিকে মোট কত টাকা দেওয়া হয়েছে
        // (PreOrderPayment গুলোর যোগফল)
        public double TotalPaid { get; set; }

        // এখনো কত টাকা দিতে বাকি = TotalAmount - TotalPaid
        // নেগেটিভ হলে বুঝতে হবে অগ্রিম বেশি দেওয়া হয়েছে
        public double TotalDue => TotalAmount - TotalPaid;

        public string Comments { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<PreOrderDetail> PreOrderDetails { get; set; }
        public virtual ICollection<PreOrderPayment> PreOrderPayments { get; set; }
        public virtual ICollection<PreOrderDelivery> PreOrderDeliveries { get; set; }
    }

    /// <summary>
    /// অর্ডারের প্রতিটি পণ্যের তথ্য (কোন পণ্য কত পিস অর্ডার, কত পিস এসেছে, কত বাকি)
    /// </summary>
    public class PreOrderDetail
    {
        public int Id { get; set; }

        public int PreOrderId { get; set; }
        public virtual PreOrder PreOrder { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        // কত পিস অর্ডার করা হয়েছে (যেমন: ১০০০)
        public int OrderedQuantity { get; set; }

        // এখন পর্যন্ত কত পিস পাওয়া গেছে (যেমন: ৯০০)
        // প্রতিবার ডেলিভারি সেভ করলে এটা বাড়বে
        public int DeliveredQuantity { get; set; }

        // কোম্পানি যে পিস আর দেবে না বলেছে (বাতিল করা পিস)
        // এটা না থাকলে "আর পাব না" এমন পণ্য চিরকাল Pending হয়ে থাকবে
        public int CancelledQuantity { get; set; }

        // এখনো কত পিস পাওনা আছে = Ordered - Delivered - Cancelled (যেমন: ১০০)
        public int PendingQuantity => OrderedQuantity - DeliveredQuantity - CancelledQuantity;

        // অর্ডারের সময়ের দাম (Product => ProductPrice থেকে নেওয়া snapshot,
        // পরে দাম বদলালেও পুরনো অর্ডারের দাম ঠিক থাকবে)
        public double CylinderUnitPrice { get; set; }
        public double GasUnitPrice { get; set; }

        // এই আইটেমের ডিসকাউন্ট (টাকা)
        public double? Discount { get; set; }

        // এই আইটেমের মোট দাম, ডিসকাউন্টের পর
        // (OrderedQuantity * ইউনিট দাম - Discount)
        public double Price { get; set; }

        // কত টাকার পণ্য এখনো পাওয়া বাকি (দেওয়া টাকা বনাম পাওয়া পণ্য মেলানোর জন্য)
        public double PendingAmount => OrderedQuantity > 0
            ? Price / OrderedQuantity * PendingQuantity
            : 0;

        public virtual ICollection<PreOrderDeliveryDetail> DeliveryDetails { get; set; }
            = new HashSet<PreOrderDeliveryDetail>();
    }

    /// <summary>
    /// অর্ডারের বিপরীতে কোম্পানিকে দেওয়া প্রতিটি পেমেন্ট
    /// (একই অর্ডারে একাধিকবার টাকা দেওয়া যাবে)
    /// </summary>
    public class PreOrderPayment
    {
        public int Id { get; set; }

        public int PreOrderId { get; set; }
        public virtual PreOrder PreOrder { get; set; }

        // কত টাকা দেওয়া হয়েছে
        public double Amount { get; set; }

        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public string Comments { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// কোম্পানি থেকে একবারে যে পণ্য বুঝে পাওয়া গেছে (ডেলিভারি রিসিপ্ট)
    /// একটা অর্ডারে একাধিক ডেলিভারি থাকতে পারে (যেমন: প্রথমে ৯০০, পরে ১০০)
    /// </summary>
    public class PreOrderDelivery
    {
        public PreOrderDelivery()
        {
            PreOrderDeliveryDetails = new HashSet<PreOrderDeliveryDetail>();
        }

        public int Id { get; set; }

        public int PreOrderId { get; set; }
        public virtual PreOrder PreOrder { get; set; }

        // পণ্য কবে বুঝে পাওয়া গেছে
        public DateTime DeliveryDate { get; set; } = DateTime.Now;

        // চালান/চালান নম্বর (কোম্পানির দেওয়া, থাকলে)
        public string ChallanNumber { get; set; }

        // যদি এই ডেলিভারি অন্য কোনো সাধারণ Purchase এর সাথে এসে থাকে
        // (যেমন: পরের অর্ডারের সাথে বাকি ১০০ পিস এসেছে) তাহলে সেই Purchase এর Id
        // সরাসরি ডেলিভারি হলে খালি থাকবে
        public int? PurchaseId { get; set; }
        public virtual Purchase Purchase { get; set; }

        public string Comments { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<PreOrderDeliveryDetail> PreOrderDeliveryDetails { get; set; }
    }

    /// <summary>
    /// প্রতিটি ডেলিভারিতে কোন পণ্য কত পিস এসেছে
    /// </summary>
    public class PreOrderDeliveryDetail
    {
        public int Id { get; set; }

        public int PreOrderDeliveryId { get; set; }
        public virtual PreOrderDelivery PreOrderDelivery { get; set; }

        // অর্ডারের কোন আইটেমের বিপরীতে এই পণ্য এসেছে
        public int PreOrderDetailId { get; set; }
        public virtual PreOrderDetail PreOrderDetail { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        // এই ডেলিভারিতে কত পিস এসেছে (যেমন: ৯০০)
        public int Quantity { get; set; }
    }
}