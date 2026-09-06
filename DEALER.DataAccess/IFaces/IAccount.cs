namespace DEALER.DataAccess
{
    public interface IAccount
    {
        int CreateUser(User user);
        bool UpdateUser(User user);
        IList<AppUsersDTO> GetUsers();
        //IList<AppRoles> GetRoles();
        User GetUser(int id);
        User GetLoginUser(LoginModel loginModel);
        bool DeleteUser(int id);
        User CheckForExistingAppUserId(string loginId);
    }
}