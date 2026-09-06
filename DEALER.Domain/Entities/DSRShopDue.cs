using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class DSRShopDue
    {
        public int Id { get; set; }

        public int? EmployeeId { get; set; } 
        public virtual Employee Employee { get; set; }

        public int? DSRCustomerId { get; set; }
        public virtual Customer DSRCustomer { get; set; }

        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; }

        public int? OrderId { get; set; }
        public virtual Order Order { get; set; }

        // পরিবর্তন: একাধিক পণ্যের জন্য লিস্ট
        public List<ShopEmptyCylinderProduct> Products { get; set; } = new List<ShopEmptyCylinderProduct>();


        public double DueAmount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

        [NotMapped]
        public bool IsSaved { get; set; } = false;

        public string DueHistory
        {
            get
            {
                string shopName = Shop?.Name ?? "";
                string area = Shop?.Area ?? "";
                string customerName = Employee?.Name ?? "";

                return $"{shopName}({area})({customerName})";
            }
        }
    }

    // নতুন ক্লাস: পণ্যের বিবরণ
    public class ShopEmptyCylinderProduct
    {
        public int Id { get; set; }
        public int DSRShopDueId { get; set; }
        public virtual DSRShopDue DSRShopDue { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int CylinderQty { get; set; }
    }
}
