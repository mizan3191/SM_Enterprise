namespace DEALER.Domain
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProductNo { get; set; }
        public bool IsDeleted { get; set; }
    }
}
