namespace DEALER.Domain
{
    public class SRDiscountDuePaymentSummary
    {
        public int SRDiscountId { get; set; }
        public string SRName { get; set; }
        public double DueAmount { get; set; }
        public double PaidAmount { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
    }
}