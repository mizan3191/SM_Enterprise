namespace DEALER.Domain
{
    public class OrdersByPersonDTO
    {
        public int OrderId { get; set; }
        public string ProductsName { get; set; }
        public string Area { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
    }
}
