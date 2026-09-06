namespace DEALER.Domain
{
    public class DamageProductReturnDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Products { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
        public double TotalPrice { get; set; }
    }

    public class ExtraReturnProductDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Products { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
        public double TotalPrice { get; set; }
    }
}
