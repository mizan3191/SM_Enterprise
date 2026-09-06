namespace DEALER.Domain
{
    public class Supplier
    {
        public Supplier()
        {
            Products = new HashSet<Product>();
            Purchases = new HashSet<Purchase>();
            SupplierPaymentHistories = new HashSet<SupplierPaymentHistory>();
        }

        public int Id { get; set; } // Primary Key
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool IsDisable { get; set; }
        public virtual ICollection<Product> Products { get; set; } // Navigation Property
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<SupplierPaymentHistory> SupplierPaymentHistories { get; set; }
    }
}
