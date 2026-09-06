//using System.ComponentModel.DataAnnotations.Schema;

//namespace DEALER.Domain
//{
//    public class DamageProductHandover
//    {
//        public DamageProductHandover() 
//        {
//            DamageProductHandoverDetails = new HashSet<DamageProductHandoverDetails>();
//        }

//        public int Id { get; set; }

//        public int? CustomerId { get; set; }
//        public virtual Customer Customer { get; set; }

//        public int SupplierId { get; set; }
//        public virtual Supplier Supplier { get; set; }

//        public double TotalPrice { get; set; }

//        public double ExtraPrice { get; set; }
//        public double DiscountPrice { get; set; }
//        public double MainPrice => TotalPrice - ExtraPrice + DiscountPrice;

//        public bool IsReceived { get; set; }
//        public bool IsDeleted { get; set; }

//        public DateTime Date { get; set; }
//        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

//        public virtual ICollection<DamageProductHandoverDetails> DamageProductHandoverDetails { get; set; } = new List<DamageProductHandoverDetails>();
//    }

//    public class DamageProductHandoverDetails
//    {
//        public int Id { get; set; }

//        public int DamageProductHandoverId { get; set; }
//        public virtual DamageProductHandover DamageProductHandover { get; set; }

//        public int ProductId { get; set; }
//        public virtual Product Product { get; set; }

//        public int Quantity { get; set; }

//        public double UnitPrice { get; set; }
//        public double TotalPrice { get; set; }

//        public string DamageReturnIdList { get; set; }
//    }
//}