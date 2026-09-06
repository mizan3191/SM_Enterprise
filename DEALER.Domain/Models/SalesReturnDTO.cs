namespace DEALER.Domain.Models
{
    public class SalesReturnDTO
    {
        public int OrderId { get; set; }
        public int SupplierId { get; set; }
        public int SellQuentity { get; set; }
        public int ReturnQuentity { get; set; }
        public double ReturnAmount { get; set; }
        public double TotalAmount { get; set; }
        public int Quentity { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
        //public string DateFormated
        //{
        //    get
        //    {
        //        return Date.ToString("MMM-dd-yy");
        //    }
        //}

        public double SellingAmount
        {
            get
            {
                return (TotalAmount - ReturnAmount);
            }
        }
        public string Area { get; set; }
        public string SupplierName { get; set; }

        public int? CartunUnit { get; set; }
        public int? BoxUnit { get; set; }

        // -------- Formatted Quantities --------
        public string QuentityFormattd
            => FormatQuantity(Quentity);

        public string SellQuentityFormattd
            => FormatQuantity(SellQuentity);

        public string ReturnQuentityFormattd
            => FormatQuantity(ReturnQuentity);


        // -------- Helper Method --------
        private string FormatQuantity(int totalQuantity)
        {
            int remaining = totalQuantity;

            int cartun = 0;
            int box = 0;
            int piece = 0;

            if (CartunUnit.HasValue && CartunUnit.Value > 0)
            {
                cartun = remaining / CartunUnit.Value;
                remaining %= CartunUnit.Value;
            }

            if (BoxUnit.HasValue && BoxUnit.Value > 0)
            {
                box = remaining / BoxUnit.Value;
                remaining %= BoxUnit.Value;
            }

            piece = remaining;

            return $"{cartun}-{box}-{piece}";
        }
    }

    public class DSRSalesReturnDTO
    {
        public int OrderId { get; set; }
        public int SupplierId { get; set; }
        public int SellQuentity { get; set; }
        public int ReturnQuentity { get; set; }
        public double ReturnAmount { get; set; }
        public double TotalAmount { get; set; }
        public int Quentity { get; set; }
        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");
        public double SellingAmount
        {
            get
            {
                return (TotalAmount - ReturnAmount);
            }
        }
        public string Area { get; set; }
        public string SupplierName { get; set; }
        public string DSRName { get; set; }

        public int? CartunUnit { get; set; }
        public int? BoxUnit { get; set; }

        // -------- Formatted Quantities --------
        public string QuentityFormattd
            => FormatQuantity(Quentity);

        public string SellQuentityFormattd
            => FormatQuantity(SellQuentity);

        public string ReturnQuentityFormattd
            => FormatQuantity(ReturnQuentity);


        // -------- Helper Method --------
        private string FormatQuantity(int totalQuantity)
        {
            int remaining = totalQuantity;

            int cartun = 0;
            int box = 0;
            int piece = 0;

            if (CartunUnit.HasValue && CartunUnit.Value > 0)
            {
                cartun = remaining / CartunUnit.Value;
                remaining %= CartunUnit.Value;
            }

            if (BoxUnit.HasValue && BoxUnit.Value > 0)
            {
                box = remaining / BoxUnit.Value;
                remaining %= BoxUnit.Value;
            }

            piece = remaining;

            return $"{cartun}-{box}-{piece}";
        }
    }

    public class SalesHistoryDTO
    {
        public string SupplierName { get; set; }
        public string ProductCategory { get; set; }
        public string ProductName { get; set; }
        public string SupplierWithCategory => SupplierName + " " + ProductCategory + " " + ProductName;

        public int Quentity { get; set; }
        public int SellQuentity { get; set; }
        public int ReturnQuentity { get; set; }

        public double Amount { get; set; }
        public double ReturnAmount { get; set; }
        public double TotalAmount => Amount + ReturnAmount;

        public int? CartunUnit { get; set; }
        public int? BoxUnit { get; set; }

        // -------- Formatted Quantities --------
        public string QuentityFormattd
            => FormatQuantity(Quentity);

        public string SellQuentityFormattd
            => FormatQuantity(SellQuentity);

        public string ReturnQuentityFormattd
            => FormatQuantity(ReturnQuentity);

        public DateTime Date { get; set; }
        public string DateFormatted => Date.ToString("dd-MMM-yyyy (ddd)");

        // -------- Helper Method --------
        private string FormatQuantity(int totalQuantity)
        {
            int remaining = totalQuantity;

            int cartun = 0;
            int box = 0;
            int piece = 0;

            if (CartunUnit.HasValue && CartunUnit.Value > 0)
            {
                cartun = remaining / CartunUnit.Value;
                remaining %= CartunUnit.Value;
            }

            if (BoxUnit.HasValue && BoxUnit.Value > 0)
            {
                box = remaining / BoxUnit.Value;
                remaining %= BoxUnit.Value;
            }

            piece = remaining;

            return $"{cartun}-{box}-{piece}";
        }
    }
}