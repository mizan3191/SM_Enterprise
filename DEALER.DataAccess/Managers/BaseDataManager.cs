using System.Security.Cryptography;
using System.Text;

namespace DEALER.DataAccess
{
    public abstract class BaseDataManager
    {
        protected DEALERContext _dbContext;

        public BaseDataManager(DEALERContext model)
        {
            _dbContext = model;
        }


        #region APIs

        protected T FindEntity<T>(int primaryKey) where T : class
        {
            try
            {
                return _dbContext.Set<T>().Find(primaryKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected T GetEntityFirstRowData<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                return _dbContext.Set<T>().Where(predicate).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected T GetEntityLastRowData<T>(Func<T, bool> predicate) where T : class
        {
            try
            {
                return _dbContext.Set<T>().LastOrDefault(predicate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected bool GetEntityAny<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                return _dbContext.Set<T>().Where(predicate).Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected IList<T> GetEntityListData<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                return _dbContext.Set<T>().Where(predicate).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected IList<T> GetEntityListData<T>() where T : class
        {
            try
            {
                return _dbContext.Set<T>().ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected T GetRowData<T>(string interpolatedStoredProc) where T : class
        {
            try
            {
                return _dbContext.Set<T>().FromSqlRaw(interpolatedStoredProc).AsEnumerable().FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected IList<T> GetLookupCollection<T>() where T : class
        {
            try
            {
                return _dbContext.Set<T>().AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected IList<T> GetListData<T>(string interpolatedStoredProc) where T : class
        {
            try
            {
                return _dbContext.Set<T>().FromSqlRaw(interpolatedStoredProc).AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                throw;
            }
        }

        protected async Task<bool> ExecuteSqlAsync(string interpolatedStoredProc)
        {
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync(interpolatedStoredProc);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected bool MarkedForDelete(string interpolatedStoredProc)
        {
            try
            {
                _dbContext.Database.ExecuteSqlRaw(interpolatedStoredProc);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected bool RemoveEntity<T>(int id) where T : class
        {
            try
            {
                var entity = _dbContext.Set<T>().Find(id);
                _dbContext.Remove<T>(entity);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected async Task<bool> AddUpdateEntityAsync<T>(T entity, bool keepDettached = true) where T : class
        {
            try
            {
                if (_dbContext.Entry<T>(entity).IsKeySet)
                    _dbContext.Update<T>(entity);
                else
                    _dbContext.Add<T>(entity);

                await _dbContext.SaveChangesAsync();

                if (keepDettached)
                {
                    _dbContext.Entry<T>(entity).State = EntityState.Detached;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected bool AddUpdateEntity<T>(T entity, bool keepDettached = true) where T : class
        {
            try
            {
                if (_dbContext.Entry<T>(entity).IsKeySet)
                    _dbContext.Update<T>(entity);
                else
                    _dbContext.Add<T>(entity);

                _dbContext.SaveChanges();

                if (keepDettached)
                {
                    _dbContext.Entry<T>(entity).State = EntityState.Detached;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        protected bool AddUpdateRange<T>(List<T> entities, bool keepDettached = true) where T : class
        {
            try
            {
                _dbContext.AddRange(entities);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }
        #endregion

        public void BalanceInTransactionHistories(int id, double amount)
        {
            if (amount == 0 || id <= 0)
                return;

            var subsequentTransactions = _dbContext.TransactionHistories
                .Where(p => p.Id > id)
                .OrderBy(p => p.Id)
                .ToList();

            foreach (var transaction in subsequentTransactions)
            {
                transaction.CurrentBalance += amount;
            }

            _dbContext.SaveChanges();
        }

        public void BalanceOutTransactionHistories(int id, double amount)
        {
            if (amount == 0 || id <= 0)
                return;

            var subsequentTransactions = _dbContext.TransactionHistories
                .Where(p => p.Id > id)
                .OrderBy(p => p.Id)
                .ToList();

            foreach (var transaction in subsequentTransactions)
            {
                transaction.CurrentBalance -= amount;
            }

            _dbContext.SaveChanges();
        }

        public string EncryptPassword(string password)
        {
            // Create Salt
            byte[] userBytes = ASCIIEncoding.ASCII.GetBytes("BeratenSoftware");
            string salt = Convert.ToBase64String(userBytes);

            // Mix Password & Salt
            string saltAndPwd = string.Concat(password, salt);

            UTF8Encoding encoder = new UTF8Encoding();
            using SHA256 sha256hasher = SHA256.Create();

            byte[] hashedDataBytes = sha256hasher.ComputeHash(encoder.GetBytes(saltAndPwd));

            string hashedPwd = Convert.ToBase64String(hashedDataBytes);
            return hashedPwd;
        }
    }
}
