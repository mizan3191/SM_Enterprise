using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DEALER.Domain
{
    public class OrderInfoDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string DateFormatted => OrderDate.ToString("dd-MMM-yyyy (ddd)");
        public string OrderDateFormated {
            get
            {
                return OrderDate.ToString("MMM-dd-yy");
            }
        }
        public string DeliveryLocation { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string HouseName { get; set; }
        public string HouseNumber { get; set; }
        public string Village { get; set; }
        public string Upozilla { get; set; }
        public string District { get; set; }
        public string Phone { get; set; }
        public string OptionalPhone { get; set; }

        [NotMapped]
        public string PhoneNumber
        {
            get
            {
                var phone = new List<string>();

                if (!string.IsNullOrEmpty(Phone))
                    phone.Add(Phone);

                if (!string.IsNullOrEmpty(OptionalPhone))
                    phone.Add(OptionalPhone);

                return string.Join(", ", phone);
            }
        }

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

                if (!string.IsNullOrEmpty(Upozilla))
                    addressParts.Add(Upozilla);

                if (!string.IsNullOrEmpty(District))
                    addressParts.Add(District);

                return string.Join(", ", addressParts);
            }
        }
    }
}
