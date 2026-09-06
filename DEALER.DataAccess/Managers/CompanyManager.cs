using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;

namespace DEALER.DataAccess
{
    public class CompanyManager : BaseDataManager, ICompany
    {
        private readonly IConfiguration _configuration;

        public CompanyManager(DEALERContext model, IConfiguration configuration) : base(model)
        {
            _configuration = configuration;
        }

        public bool UpdateCompanyInfo(CompanyInfo CompanyInfo)
        {
            return AddUpdateEntity(CompanyInfo);
        }

        public int CreateCompanyInfo(CompanyInfo CompanyInfo)
        {
            AddUpdateEntity(CompanyInfo);
            return CompanyInfo.Id;
        }

        public CompanyInfo GetCompanyInfo()
        {
            try
            {
                return _dbContext.CompanyInfos.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<byte[]> CreateBackupZipAsync()
        {
            var connectionString = _dbContext.Database.GetConnectionString();
            var builder = new SqlConnectionStringBuilder(connectionString);
            var dbName = builder.InitialCatalog;

            // **আপনার কাস্টম পাথ - যেখানে ফাইল সেভ হবে**
            var backupRootFolder = _configuration["BackupSettings:BackupRootPath"];
            // যেমন: "D:\\MyBackups"

            if (string.IsNullOrWhiteSpace(backupRootFolder))
            {
                throw new InvalidOperationException("BackupSettings:BackupRootPath appsettings.json e set kora nei.");
            }

            // ফোল্ডার তৈরি করুন যদি না থাকে
            if (!Directory.Exists(backupRootFolder))
            {
                Directory.CreateDirectory(backupRootFolder);
            }

            // তারিখ অনুযায়ী সাব-ফোল্ডার তৈরি করুন (optional)
            var dateFolder = Path.Combine(backupRootFolder, DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(dateFolder))
            {
                Directory.CreateDirectory(dateFolder);
            }

            try
            {
                var bakFileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var bakFilePath = Path.Combine(dateFolder, bakFileName);

                // Backup SQL
                var backupSql = $@"
            BACKUP DATABASE [{dbName}] 
            TO DISK = N'{bakFilePath}' 
            WITH FORMAT, INIT, 
            NAME = N'{dbName}-Full Backup', 
            SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                await using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    await using var cmd = new SqlCommand(backupSql, conn) { CommandTimeout = 600 };
                    await cmd.ExecuteNonQueryAsync();
                }

                // Zip file তৈরি করুন
                var zipFileName = $"{dbName}_backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                var zipFilePath = Path.Combine(dateFolder, zipFileName);

                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(bakFilePath, bakFileName);
                }

                // .bak file ডিলিট করুন (শুধু zip রাখতে চাইলে)
                if (File.Exists(bakFilePath))
                {
                    File.Delete(bakFilePath);
                }

                // **এখন ফাইলটা আপনার কাস্টম পাথে সেভ হয়েছে**
                // browser এ download করানোর জন্য bytes রিটার্ন করুন
                var zipBytes = await File.ReadAllBytesAsync(zipFilePath);
                return zipBytes;
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup failed: {ex.Message}");
            }
        }
    }
}