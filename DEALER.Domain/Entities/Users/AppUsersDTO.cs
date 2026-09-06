namespace DEALER.Domain
{
    public class AppUsersDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Roles Role { get; set; }
        public string UserName { get; set; }
    }
}
