namespace DEALER.Domain
{

    public class CustomerOrderDetailDTO
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public double GasUnitPrice { get; set; }
        public double CylinderUnitPrice { get; set; }
        public int Quantity { get; set; }
        public int ReturnQuantity { get; set; }
        public double Discount { get; set; }
        public double Price { get; set; }

        // Order-level
        public double OrderTotalAmount { get; set; }
        public double OrderTotalPay { get; set; }
        public double OrderTotalDue { get; set; }
        public double OrderDiscount { get; set; }
        public double DeliveryCharge { get; set; }
    }


    public class CustomerInfo
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

    }
}
