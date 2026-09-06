namespace DEALER.Domain
{
    public class ProductConsumptionDTO
    {
        public int Id { get; set; } // Primary Key     
        public int QuantityConsumed { get; set; }
        public string ReasonOfConsumed { get; set; }
        public DateTime DateConsumed { get; set; }
        public string DateFormatted => DateConsumed.ToString("dd-MMM-yyyy (ddd)");
        public string ProductName { get; set; }
        //public string Person { get; set; }
    }
}
