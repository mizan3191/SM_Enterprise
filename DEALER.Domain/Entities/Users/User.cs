using System.ComponentModel;

namespace DEALER.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Roles UserRole { get; set; }
        public bool IsDisable { get; set; }
    }

    public enum Roles
    {
        [Category("SA")]
        Administrator = 1,
        [Category("AD")]
        Admin = 2,
        [Category("MG")]
        Manager = 3,
        [Category("SM")]
        Sales = 4,
    }

    public enum UserGroupEnum
    {
        None = 0,
        SA = 1,
        AD = 2,
        MG = 3,
        SM = 4,
    }
}