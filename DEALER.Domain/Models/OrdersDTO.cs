namespace DEALER.Domain
{
    public class OrdersDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
        //public string OrderDateFormate => OrderDate.ToString("dd-MM-yyyy");
        public double TotalPrice { get; set; }
        public string Address { get; set; }
        public bool IsLock { get; set; }
    }
}