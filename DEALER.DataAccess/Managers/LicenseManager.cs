using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Cryptography;

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
            var token = _config["LicenseSettings:ExpiryToken"];

            // 1) Key মুছে দিলে / খালি রাখলে / পুরো LicenseSettings সেকশন মুছলে → লক
            if (string.IsNullOrWhiteSpace(token))
            {
                return Invalid("লাইসেন্স পাওয়া যায়নি। যোগাযোগ করুন।");
            }

            try
            {
                // 2) এলোমেলো অক্ষর যোগ/মুছলে এখানে exception হবে
                var decrypted = LicenseCrypto.Decrypt(token.Trim());

                // 3) Signature যাচাই, যাতে garbage decrypt হলেও ধরা পড়ে
                var parts = decrypted.Split('|');
                if (parts.Length != 2 || parts[1] != "DEALER")
                    throw new CryptographicException("Invalid license payload");

                var expiryDate = DateTime.ParseExact(parts[0], "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

                return new LicenseInfo
                {
                    ExpiryDate = expiryDate,
                    Message = $"লাইসেন্স মেয়াদ শেষ হয়েছে ({expiryDate:dd-MM-yyyy})। যোগাযোগ করুন।"
                };
            }
            catch
            {
                // 4) যেকোনো ভুল (FormatException, CryptographicException, ParseExact ইত্যাদি) → লক
                return Invalid("লাইসেন্স অবৈধ। যোগাযোগ করুন।");
            }
        }

        public bool IsLicenseExpired()
        {
            var license = GetLicenseInfo();
            return DateTime.Now.Date > license.ExpiryDate.Date;
        }

        private static LicenseInfo Invalid(string message) => new LicenseInfo
        {
            ExpiryDate = DateTime.MinValue,
            Message = message
        };
    }
}

