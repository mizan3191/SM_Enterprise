using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DEALER.Domain
{
    public abstract class BaseLookup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class Packaging : BaseLookup {}
    public class UnitOfMeasurement : BaseLookup {}
    public class ProductsSize : BaseLookup {}
    public class ReasonofAdjustment : BaseLookup { }
    public class Road : BaseLookup { }
    public class ExpenseType : BaseLookup { }
    public class DailyExpenseType : BaseLookup { }
    public class CustomerType : BaseLookup { }
    public class EmployeeType : BaseLookup { }

    public class PaymentMethod : BaseLookup { } 
    public class ShippingMethod : BaseLookup { }

    public class Shop : BaseLookup 
    {
        public string Area {  get; set; }
        public string ShopOwner {  get; set; }
        public string Number {  get; set; }
    }
}
