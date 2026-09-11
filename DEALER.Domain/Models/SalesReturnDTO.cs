namespace DEALER.Domain.Models
{
    public class SalesReturnDTO
    {
        public int OrderId { get; set; }
        public int SupplierId { get; set; }
        public int SellQuentity { get; set; }
        public int ReturnQuentity { get; set; }
        public double ReturnAmount { get; set; }
        public double TotalAmount { get; set; }
        public int Quentity { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
      
        public double SellingAmount
        {
            get
            {
                return (TotalAmount - ReturnAmount);
            }
        }
        public string Area { get; set; }
        public string SupplierName { get; set; }
    }

    public class DSRSalesReturnDTO
    {
        public int OrderId { get; set; }
        public int SupplierId { get; set; }
        public int SellQuentity { get; set; }
        public int ReturnQuentity { get; set; }
        public double ReturnAmount { get; set; }
        public double TotalAmount { get; set; }
        public int Quentity { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
        public double SellingAmount
        {
            get
            {
                return (TotalAmount - ReturnAmount);
            }
        }
        public string Area { get; set; }
        public string SupplierName { get; set; }
        public string DSRName { get; set; }

    }

    public class SalesHistoryDTO
    {
        public string SupplierName { get; set; }
        public string ProductName { get; set; }
        public string SupplierWithCategory => SupplierName + " " + ProductName;

        public int Quentity { get; set; }
        public int SellQuentity { get; set; }
        public int ReturnQuentity { get; set; }

        public double Amount { get; set; }
        public double ReturnAmount { get; set; }
        public double TotalAmount => Amount + ReturnAmount;


        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
    }
}