namespace DEALER.Domain
{
    public class CylinderExchange
    {
        public CylinderExchange()
        {
            ExchangeDetails = new HashSet<CylinderExchangeDetail>();
            ExchangePaymentHistories = new HashSet<ExchangePaymentHistory>();
        }

        public int Id { get; set; }

        public int? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }

        // "Give" / "Receive"
        public string ExchangeType { get; set; } = string.Empty;

        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        // Subtotal (discount er age)
        public double SubTotal { get; set; }

        // Exchange-wise discount (amount)
        public double ExchangeDiscount { get; set; }

        // Final total (SubTotal - ProductDiscount - ExchangeDiscount)
        public double TotalAmount { get; set; }

        public double TotalPay { get; set; }
        public double TotalDue { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }
        public string Comments { get; set; }

        public virtual ICollection<CylinderExchangeDetail> ExchangeDetails { get; set; }
        public virtual ICollection<ExchangePaymentHistory> ExchangePaymentHistories { get; set; }
    }

    public class CylinderExchangeDetail
    {
        public int Id { get; set; }

        public int CylinderExchangeId { get; set; }
        public virtual CylinderExchange CylinderExchange { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }

        // ProductPrice theke snapshot
        public double CylinderUnitPrice { get; set; }

        // Product-wise discount (%)
        public double DiscountPercent { get; set; }

        // Discount amount (calculated)
        public double DiscountAmount { get; set; }

        // Line total (after discount)
        public double TotalPrice => (Quantity * CylinderUnitPrice) - DiscountAmount;
    }

    public class ExchangePaymentHistory
    {
        public int Id { get; set; }

        public int CylinderExchangeId { get; set; }
        public virtual CylinderExchange CylinderExchange { get; set; }

        public double Amount { get; set; }
        public string Direction { get; set; } = string.Empty;

        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public string Comments { get; set; }
        public bool IsDeleted { get; set; }
    }

    // ================= VIEW MODELS =================
    public class CylinderExchangeDetailViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double CylinderUnitPrice { get; set; }

        // Product-wise discount %
        public double DiscountPercent { get; set; }

        // Calculated
        public double SubTotal => Quantity * CylinderUnitPrice;
        public double DiscountAmount => SubTotal * (DiscountPercent / 100);
        public double TotalPrice => SubTotal - DiscountAmount;
    }

    public class DropdownItem
    {
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class CylinderExchangeDetailsDTO
    {
        public int CylinderExchangeId { get; set; }

        // Header info
        public string SupplierName { get; set; } = string.Empty;
        public string ExchangeType { get; set; } = string.Empty;
        public DateTime ExchangeDate { get; set; }
        public string DateFormatted => ExchangeDate.ToString("dd-MMM-yyyy");

        // Detail info
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public double CylinderUnitPrice { get; set; }

        // Product-wise discount
        public double DiscountPercent { get; set; }
        public double DiscountAmount { get; set; }

        public double TotalPrice { get; set; }

        // Header totals (repeated for convenience)
        public double SubTotal { get; set; }
        public double ExchangeDiscount { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPay { get; set; }
        public double TotalDue { get; set; }
        public string? Comments { get; set; }
    }
}