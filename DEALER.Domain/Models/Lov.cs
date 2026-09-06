namespace DEALER.Domain
{
    public class Lov
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public bool IsDeleted { get; set; }
    }

    public class Lovd : Lov
    {
        public string Desc { get; set; }
    }

    public class Ledger
    {
        public double Id { get; set; }
        public string Name { get; set; }
    }
}
