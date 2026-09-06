using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DEALER.Domain
{
    public class ProductConsumption
    {
        public int Id { get; set; } // Primary Key     
        public int QuantityConsumed { get; set; }
        public DateTime DateConsumed { get; set; }
        public string DateFormatted => DateConsumed.ToString("dd-MMM-yyyy (ddd)");

        public string Comments { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Property
        public int ProductId { get; set; } // Foreign Key to Product
        public virtual Product Product { get; set; }

        // Navigation Property
        public int CustomerId { get; set; } // Foreign Key to Product
        //public virtual Customer Customer { get; set; }

        public int ReasonofAdjustmentId { get; set; } // Foreign Key to ReasonofAdjustment
        public virtual ReasonofAdjustment ReasonofAdjustment { get; set; }
    }
}
