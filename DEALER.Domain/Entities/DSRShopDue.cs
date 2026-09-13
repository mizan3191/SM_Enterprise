using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class DSRShopDue
    {
        public int Id { get; set; }

        public int? EmployeeId { get; set; } 
        public virtual Employee Employee { get; set; }

        public int? DSREmployeeId { get; set; }
        public virtual Employee DSREmployee { get; set; }

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
        public double DueAmount { get; set; } // here will be the due amount for this specific product Product.CurrentPrice.EmptyCylinderPrice * CylinderQty
    }


    // Shop Due with Cylinder Details
    public class ShopDueWithCylinderDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopArea { get; set; }
        public string CustomerName { get; set; }
        public double DueAmount { get; set; }
        public DateTime Date { get; set; }
        public List<ShopCylinderDetailDto> CylinderDetails { get; set; } = new();
        public int TotalCylinders => CylinderDetails.Sum(c => c.Quantity);
    }

    // Individual Cylinder Detail for a Shop
    public class ShopCylinderDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int Quantity { get; set; }
        public double EmptyCylinderPrice { get; set; }
        public double TotalValue => Quantity * EmptyCylinderPrice;
    }

    // Grouped by Supplier (Company)
    public class ShopCylinderGroupDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int TotalQuantity { get; set; }
        public List<ShopCylinderDetailDto> Details { get; set; } = new();
    }
}
