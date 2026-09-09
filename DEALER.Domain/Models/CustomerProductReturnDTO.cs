using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DEALER.Domain
{
    public class CustomerProductReturnDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string EmployeeName { get; set; }
        public string Products { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
        public double TotalPrice { get; set; }
    }
}