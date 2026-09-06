namespace DEALER.Domain
{
    public class PreOrderDetailViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public double Price { get; set; }
        public string UnitofM { get; set; }
        public double UnitofPrice { get; set; }
        public double Discount { get; set; }
        public double DiscountAmount { get; set; }

        public int FreeQuantity { get; set; }

        public FreeProductOfferDTO FreeProductOffer { get; set; }
    }   
}