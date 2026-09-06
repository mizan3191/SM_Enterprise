namespace DEALER.Domain
{
    public class PreOrdersDTO
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
        public double TotalPrice { get; set; }
        
    }
}