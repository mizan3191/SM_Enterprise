using Microsoft.EntityFrameworkCore;
using DEALER.Domain;

namespace DEALER.Infrastructure
{
    public class DEALERContext : DbContext
    {
        public DEALERContext(DbContextOptions<DEALERContext> options) : base(options) { }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<CompanyInfo> CompanyInfos { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Product> Products { get; set; }
       // public virtual DbSet<FreeProductOffer> FreeProductOffers { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<CustomerProductReturnDetails> CustomerProductReturnDetails { get; set; }
        public virtual DbSet<CustomerProductReturn> CustomerProductReturns { get; set; }
        public virtual DbSet<CustomerPaymentHistory> CustomerPaymentHistories { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<PriceHistory> PriceHistories { get; set; }
        public virtual DbSet<ProductConsumption> ProductConsumptions { get; set; }
        public virtual DbSet<Purchase> Purchases { get; set; }
        public virtual DbSet<PurchaseDetail> PurchaseDetails { get; set; }
        public virtual DbSet<SupplierPaymentHistory> SupplierPaymentHistories { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
        //public virtual DbSet<PreOrder> PreOrders { get; set; }
      //  public virtual DbSet<PreOrderDetails> PreOrderDetails { get; set; }
       // public virtual DbSet<DamageProductReturn> DamageProductReturns { get; set; }
       // public virtual DbSet<DamageProductReturnDetails> DamageProductReturnDetails { get; set; }
      //  public virtual DbSet<SRDiscount> SRDiscounts { get; set; }
       // public virtual DbSet<SRPaymentHistory> SRPaymentHistories { get; set; }
        public virtual DbSet<DSRShopDue> DSRShopDues { get; set; }
        public virtual DbSet<ShopEmptyCylinderProduct> ShopEmptyCylinderProducts { get; set; }
        public virtual DbSet<DSRShopPaymentHistory> DSRShopPaymentHistories { get; set; }
        //public virtual DbSet<DamageProduct> DamageProducts { get; set; }
        public virtual DbSet<OrderPaymentHistory> OrderPaymentHistories { get; set; }
        public virtual DbSet<DailyExpense> DailyExpenses { get; set; }
       // public virtual DbSet<DamageProductHandover> DamageProductHandovers { get; set; }
        //public virtual DbSet<DamageProductHandoverDetails> DamageProductHandoverDetails { get; set; }
       // public virtual DbSet<DamageProductHandoverPaymentHistory> DamageProductHandoverPaymentHistories { get; set; }
        public virtual DbSet<TransactionHistory> TransactionHistories { get; set; }
        public virtual DbSet<ExtraReturnProduct> ExtraReturnProducts { get; set; }
        public virtual DbSet<ExtraReturnProductDetails> ExtraReturnProductDetails { get; set; }

        public virtual DbSet<ProductStock> ProductStocks { get; set; }
        public virtual DbSet<ProductPrice> ProductPrices { get; set; }

        #region Lookup Tables        
        public virtual DbSet<UnitOfMeasurement> UnitOfMeasurement { get; set; }
        public virtual DbSet<ReasonofAdjustment> ReasonofAdjustment { get; set; }
        //public virtual DbSet<Packaging> Packaging { get; set; }
        public virtual DbSet<ProductsSize> ProductsSize { get; set; }
        public virtual DbSet<Road> Road { get; set; }
        public virtual DbSet<ExpenseType> ExpenseType { get; set; }
        public virtual DbSet<DailyExpenseType> DailyExpenseType { get; set; }
        public virtual DbSet<CustomerType> CustomerType { get; set; }
        public virtual DbSet<EmployeeType> EmployeeType { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethod { get; set; }
        public virtual DbSet<ShippingMethod> ShippingMethod { get; set; }
        public virtual DbSet<Shop> Shop { get; set; }

        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<FreeProductOffer>()
            //   .HasOne(f => f.Product)          // FreeProductOffer has one Product (main product)
            //   .WithMany()                      // Product can be in many FreeProductOffers
            //   .HasForeignKey(f => f.ProductId)  // Foreign key is ProductId
            //   .OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete

            //modelBuilder.Entity<FreeProductOffer>()
            //    .HasOne(f => f.FreeProduct)       // FreeProductOffer has one FreeProduct (gift)
            //    .WithMany()                       // FreeProduct can be in many FreeProductOffers
            //    .HasForeignKey(f => f.FreeProductId) // Foreign key is FreeProductId
            //    .OnDelete(DeleteBehavior.Restrict);  // Prevents cascade delete

            //// Ensure EF doesn't create extra columns
            //modelBuilder.Entity<FreeProductOffer>()
            //    .Ignore(f => f.ProductId1); // Explicitly ignore any auto-generated shadow property

            //modelBuilder.Entity<DamageProductHandoverDetails>()
            //    .HasOne(d => d.Product)
            //    .WithMany(p => p.DamageProductHandoverDetails)
            //    .HasForeignKey(d => d.ProductId)
            //    .OnDelete(DeleteBehavior.Restrict); // or .NoAction

            //modelBuilder.Entity<CustomerProductReturn>()
            //    .HasOne(cpr => cpr.Order)
            //    .WithMany() // If Order has no collection back to CustomerProductReturn
            //    .HasForeignKey(cpr => cpr.OrderId)
            //    .OnDelete(DeleteBehavior.Restrict); // Or your preferred delete behavior

            // Configure other relationships explicitly
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<OrderPaymentHistory>()
            //     .HasOne(o => o.Customer)
            //     .WithMany(c => c.OrderPaymentHistories)
            //     .HasForeignKey(o => o.CustomerId)
            //     .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrderPaymentHistory>()
                .HasOne(o => o.DSREmployee)
                .WithMany()
                .HasForeignKey(o => o.DSREmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<SRDiscount>()
            //   .HasOne(s => s.Customer)
            //   .WithMany()
            //   .HasForeignKey(s => s.CustomerId)
            //   .OnDelete(DeleteBehavior.Restrict); // or whatever delete behavior you want

            //modelBuilder.Entity<SRDiscount>()
            //    .HasOne(s => s.DSRCustomer)
            //    .WithMany()
            //    .HasForeignKey(s => s.DSRCustomerId)
            //    .OnDelete(DeleteBehavior.Restrict); // or whatever delete behavior you want



        }
    }
}
