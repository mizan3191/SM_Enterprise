namespace DEALER.Domain
{
    public class RevenueDTO
    {
        public string Quarter { get; set; }
        public int NUmberOfProducts { get; set; }
    }

    public class ProductSalesDTO
    {
        public DateTime Date { get; set; }
        public int QuantitySold { get; set; }
        public string FormattedDate { get; set; }
    }
}
