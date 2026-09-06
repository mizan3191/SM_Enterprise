using System.ComponentModel.DataAnnotations.Schema;

namespace DEALER.Domain
{
    public class CustomerDueDTO
    {
        public int Id { get; set; }

        public double TotalDue { get; set; }
        public string Name { get; set; }
        public string HouseName { get; set; }
        public string HouseNumber { get; set; }
        public string Village { get; set; }
        public string Upazilla { get; set; }
        public string District { get; set; }
        public string Phone { get; set; }

        [NotMapped]
        public string FullAddress
        {
            get
            {
                var addressParts = new List<string>();

                if (!string.IsNullOrEmpty(HouseName))
                    addressParts.Add(HouseName);

                if (!string.IsNullOrEmpty(HouseNumber))
                    addressParts.Add(HouseNumber);

                if (!string.IsNullOrEmpty(Village))
                    addressParts.Add(Village);

                if (!string.IsNullOrEmpty(Upazilla))
                    addressParts.Add(Upazilla);

                if (!string.IsNullOrEmpty(District))
                    addressParts.Add(District);

                return string.Join(", ", addressParts);
            }
        }
    }
}
