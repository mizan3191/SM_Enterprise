namespace DEALER.DataAccess
{
    public class AccountManager : BaseDataManager, IAccount
    {
        public AccountManager(DEALERContext model) : base(model)
        {
        }

        public int CreateUser(User user)
        {
            user.Password = EncryptPassword(user.Password);
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return user.Id;

        }

        public bool UpdateUser(User appUserData)
        {
            return AddUpdateEntity(appUserData);
        }

        public bool DeleteUser(int id)
        {
            return RemoveEntity<User>(id);
        }

        public User GetLoginUser(LoginModel loginModel)
        {
            var password = EncryptPassword(loginModel.Password);
            var entity = _dbContext.Users.FirstOrDefault(x => x.UserName == loginModel.UserName && x.Password == password && x.IsDisable == false);

            if (entity == null)
            {
                return null;
            }

            return entity;
        }

        public User GetUser(int id)
        {
            return FindEntity<User>(id);
        }

        public IList<AppUsersDTO> GetUsers()
        {

            return (from A in _dbContext.Users
                    select new AppUsersDTO
                    {
                        Id = A.Id,
                        UserName = A.UserName,
                        Name = A.Name,
                        Email = A.Email,
                        Role = A.UserRole
                    }).ToList();
        }

        public User CheckForExistingAppUserId(string loginId)
        {
            return GetEntityFirstRowData<User>(x => x.UserName.ToUpper() == loginId.ToUpper());
        }
    }
}