namespace DEALER.Domain
{
    public class DamageProductHandoverDetailsDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } // Assuming Customer has a Name property
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } // Assuming Supplier has a Name property
        public double TotalPrice { get; set; }
        public double ExtraPrice { get; set; }
        public double DiscountPrice { get; set; }
        public double MainPrice { get; set; }
        public bool IsReceived { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
    }

    public class DamageProductHandoverListDetailsDTO
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
    }
}
