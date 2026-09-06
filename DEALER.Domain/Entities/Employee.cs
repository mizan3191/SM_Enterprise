namespace DEALER.Domain
{
    public class Customer
    {

        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }

        public int? CustomerTypeId { get; set; }
        public CustomerType CustomerType { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsDisable { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }

        public int? EmployeeTypeId { get; set; }
        public EmployeeType EmployeeType { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public bool IsDisable { get; set; }
    }
}