using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DEALER.Domain
{
    public class Order
    {
        public Order() 
        {
            OrderDetails = new HashSet<OrderDetail>();
            CustomerPaymentHistories = new HashSet<CustomerPaymentHistory>();
            OrderPaymentHistories = new HashSet<OrderPaymentHistory>();
            DailyExpenses = new HashSet<DailyExpense>();
            DSRShopDues = new HashSet<DSRShopDue>();
            CustomerProductReturns = new HashSet<CustomerProductReturn>();
            ExtraReturnProducts = new HashSet<ExtraReturnProduct>();
        }

        public int Id { get; set; }

        public int? EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        //public string TransactionID { get; set; }
        //public string Number { get; set; }

        public double DeliveryCharge { get; set; }
        public string DeliveryLocation { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPay { get; set; }
        public double Discount { get; set; }
        public double TotalDue { get; set; }

        public bool IsDeleted { get; set; }

        public string SelectedRoad { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<CustomerPaymentHistory> CustomerPaymentHistories { get; set; }
        public virtual ICollection<OrderPaymentHistory> OrderPaymentHistories { get; set; }
        public virtual ICollection<DailyExpense> DailyExpenses { get; set; }
        public virtual ICollection<DSRShopDue> DSRShopDues { get; set; }
        public virtual ICollection<CustomerProductReturn> CustomerProductReturns { get; set; }
        public virtual ICollection<ExtraReturnProduct> ExtraReturnProducts { get; set; }
    }

    public class OrderInfo
    {
        public int OrderId { get; set; }
        public string Area { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");

    }

    public class ExistingOrderDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int StockQty { get; set; }
    }

}