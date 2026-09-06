namespace DEALER.Domain
{
    public class DamageProductHandoverDTO
    {
        public int Id { get; set; }
        public string DamageReturnIdList { get; set; }

        public int ProductId { get; set; }
        public string Products { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
    }
}