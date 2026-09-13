using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class CustomerDueDTO
    {
        public int Id { get; set; }

        public double TotalDue { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
