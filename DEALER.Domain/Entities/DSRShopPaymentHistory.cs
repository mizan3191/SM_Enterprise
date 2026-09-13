using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DEALER.Domain
{
    public class DSRShopPaymentHistory
    {
        public int Id { get; set; }
        public double AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string DateFormatted => PaymentDate.ToString("dd-MMM-yyyy (ddd)");

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        // পরিবর্তন: একাধিক পণ্যের জন্য লিস্ট
        public List<EmptyCylinderPaymentHistory> Products { get; set; } = new List<EmptyCylinderPaymentHistory>();


        public int? PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }

        public string PaymentMethodFormatted => PaymentMethod?.Name ?? "";

        public int ShopId { get; set; }
        public virtual Shop Shop { get; set; }
        public bool IsDeleted { get; set; }


        [NotMapped]
        public bool IsSaved { get; set; } = false;
    }

    public class EmptyCylinderPaymentHistory
    {
        public int Id { get; set; }

        // DSRShopDue এর সাথে সম্পর্ক (nullable, কারণ সব প্রোডাক্ট Due এর সাথে থাকবে না)
        public int? DSRShopPaymentHistoryId { get; set; }
        public virtual DSRShopPaymentHistory DSRShopPaymentHistory { get; set; }


        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int CylinderQty { get; set; }
        public double PaidAmount { get; set; } // (Product.CurrentPrice.EmptyCylinderPrice * CylinderQty) + ExtraPaidAmount
        public double ExtraPaidAmount { get; set; } 
    }

    public class DSRDueSummary
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string OwnerName { get; set; }
        public string Area { get; set; }
        public string Number { get; set; }
        public double TotalShopDueAmount { get; set; }
        public double TotalCylinderDueAmount { get; set; }
    }

    public class ShopDuePaymentSummary
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string OwnerName { get; set; }
        public string Area { get; set; }
        public double ShopDueAmount { get; set; }
        public double CylinderDueAmount { get; set; } //newly added
        public double ShopPaidAmount { get; set; }
        public double CylinderPaidAmount { get; set; } //newly added
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
    }
    public class TempShopSummary
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string OwnerName { get; set; }
        public string Area { get; set; }
        public DateTime Date { get; set; }
        public double DueAmount { get; set; }
        public double CylinderDueAmount { get; set; } // newly added
        public double PaidAmount { get; set; }
        public double CylinderPaidAmount { get; set; } // newly added
    }


    public class ShopDuePaymentListSummary
    {
        public int ShopId { get; set; }
        public int OrderId { get; set; }
        public int EmployeeId { get; set; }
        public string ShopName { get; set; }
        public string OwnerName { get; set; }
        public string Area { get; set; }
        public string ReferredBy { get; set; }
        public double CylinderDueAmount { get; set; } // newly added
        public double CylinderPaidAmount { get; set; } // newly added

        public double ShopDueAmount { get; set; }
        public double ShopPaidAmount { get; set; }
        public DateTime Date { get; set; }
        public string DateFormated => Date.ToString("dd/MM/yyyy");
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
    }

    public class TempShopDuePaymentListSummary
    {
        public int ShopId { get; set; }
        public int OrderId { get; set; }
        public int EmployeeId { get; set; }
        public string ShopName { get; set; }
        public string OwnerName { get; set; }
        public string Area { get; set; }
        public string ReferredBy { get; set; }
        public DateTime Date { get; set; }
        public double CylinderDueAmount { get; set; } // newly added
        public double CylinderPaidAmount { get; set; } // newly added
        public double DueAmount { get; set; }
        public double PaidAmount { get; set; }
    }
}
