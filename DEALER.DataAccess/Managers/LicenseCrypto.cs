using System.Security.Cryptography;
using System.Text;

namespace DEALER.DataAccess
{
    public static class LicenseCrypto
    {
        // এই Key/IV টা শুধু আপনার আর আমার মধ্যে থাকবে — সোর্স কোডে hardcoded, appsettings.json-এ না
        private static readonly byte[] Key = Convert.FromBase64String("tdTotvpj5cDxKVXZtL84RutnWtzyJFYAleSbMHHPrNc=");
        private static readonly byte[] IV = Convert.FromBase64String("FxkvwzGCNFrMVx9CXSzkAw==");

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }

        public static string Decrypt(string cipherTextBase64)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherTextBase64);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}


