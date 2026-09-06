using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class FreeProductOffer
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int BuyQuantity { get; set; }
        public int FreeQuantity { get; set; }

        public bool IsActive { get; set; }

        // NEW: What kind of gift it is
        public GiftType GiftType { get; set; }

        // NEW: Depending on GiftType, these IDs will be used
        public int? FreeProductId { get; set; }
        public virtual Product FreeProduct { get; set; }

        public string CustomItem { get; set; } // e.g., "Gift Box", "Keychain"
    }

    public class FreeProductOfferDTO
    {
        public int BuyQuantity { get; set; }
        public int FreeQuantity { get; set; }
        public string ProductName { get; set; }
    }

    public enum GiftType
    {
        [Display(Name = "Same Product")]
        SameProduct = 1,

        [Display(Name = "Different Product")]
        DifferentProduct = 2,

        [Display(Name = "Custom Item")]
        CustomItem = 3
    }
}