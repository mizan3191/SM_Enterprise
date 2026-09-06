namespace DEALER.Domain
{
    public abstract class DSRShopDueDTOBase
    {
        public int Id { get; set; }

        public string EmployeeName { get; set; }
        
        public string ShopName { get; set; }
        public double DueAmount { get; set; }
        public DateTime? Date { get; set; }
        public string DateFormatted => Date.HasValue
                                        ? Date.Value.ToString("dd-MMM-yyyy (ddd)")
                                        : string.Empty;
    }

    public class DSRShopDueDTO : DSRShopDueDTOBase
    {
        public string IssuedByName { get; set; }
        public string ShopShopOwner { get; set; }
        public string ShopArea { get; set; }
        public string ShopNumber { get; set; }
        public int? OrderId { get; set; }
        
    }

    public class DSRShopDueForOrderDTO : DSRShopDueDTOBase
    {
        public string DSREmployeeName { get; set; }
        public int? OrderId { get; set; }
        public int? TotalCylinderQty { get; set; }
        public List<ShopEmptyCylinderProductDTO> Products { get; set; } = new List<ShopEmptyCylinderProductDTO>();
    }

    public class ShopEmptyCylinderProductDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CylinderQty { get; set; }
    }
}
