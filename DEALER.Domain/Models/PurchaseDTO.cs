namespace DEALER.Domain
{
    public class PurchaseDTO
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        public string ShippingMethod { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
        public double TotalPrice { get; set; }
        public double DamageProductDueAdjustment { get; set; }
    }
}