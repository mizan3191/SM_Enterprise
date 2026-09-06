using Microsoft.Extensions.Configuration;

namespace DEALER.DataAccess
{
    public class LicenseManager : ILicense
    {
        private readonly IConfiguration _config;

        public LicenseManager(IConfiguration config)
        {
            _config = config;
        }

        public LicenseInfo GetLicenseInfo()
        {
            try
            {
                var token = _config["LicenseSettings:ExpiryToken"];
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = "/x94lwSOA0ijfSU9ANm+Pw==";
                }
                //In Every month in a specific day the date will be reset to a new date, so the token will be decrypted to a new date.
                var decrypted = LicenseCrypto.Decrypt(token); // e.g. "2026-09-29"
                var expiryDate = DateTime.Parse(decrypted);

                return new LicenseInfo
                {
                    ExpiryDate = expiryDate,
                    Message = $"লাইসেন্স মেয়াদ শেষ হয়েছে ({expiryDate:dd-MM-yyyy})। যোগাযোগ করুন।"
                };
            }
            catch
            {
                return null;
            }
        }

        public bool IsLicenseExpired()
        {
            var license = GetLicenseInfo();
            if (license == null) return false; // token না থাকলে/corrupt হলে block করব না
            return DateTime.Now.Date > license.ExpiryDate.Date;
        }
    }
}