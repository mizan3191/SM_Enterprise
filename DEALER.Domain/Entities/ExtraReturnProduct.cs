// DEALER.Domain/ExtraReturnProduct.cs
namespace DEALER.Domain
{
    public class ExtraReturnProduct
    {
        public ExtraReturnProduct()
        {
            ExtraReturnProductDetails = new HashSet<ExtraReturnProductDetails>();
            CustomerPaymentHistories = new HashSet<CustomerPaymentHistory>();
        }

        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public double TotalAmount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
        public string Remarks { get; set; }

        public virtual ICollection<ExtraReturnProductDetails> ExtraReturnProductDetails { get; set; }
        public virtual ICollection<CustomerPaymentHistory> CustomerPaymentHistories { get; set; }
    }

    public class ExtraReturnProductDetails
    {
        public int Id { get; set; }
        public int ExtraReturnProductId { get; set; }
        public virtual ExtraReturnProduct ExtraReturnProduct { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int Quantity { get; set; } // Empty cylinder quantity

        // Empty cylinder price (different for each company)
        public double EmptyCylinderPrice { get; set; }

        // Total price = Quantity × EmptyCylinderPrice
        public double Price { get; set; }
    }

    public class ExtraReturnProductDetailViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; } = 1;
        public double EmptyCylinderPrice { get; set; }
        public double Price { get; set; }
    }
}