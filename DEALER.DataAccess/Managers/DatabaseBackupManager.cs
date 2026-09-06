using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;

namespace DEALER.DataAccess
{
    public class DatabaseBackupManager : IDatabaseBackupManager
    {
        private readonly string _connectionString;
        private readonly string _dbName;

        public DatabaseBackupManager(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            var builder = new SqlConnectionStringBuilder(_connectionString);
            _dbName = builder.InitialCatalog;
        }

        public async Task<byte[]> CreateBackupZipAsync()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            try
            {
                var bakFileName = $"{_dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var bakFilePath = Path.Combine(tempFolder, bakFileName);

                var backupSql = $@"
                BACKUP DATABASE [{_dbName}] 
                TO DISK = N'{bakFilePath}' 
                WITH FORMAT, INIT, 
                NAME = N'{_dbName}-Full Backup', 
                SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                await using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    await using var cmd = new SqlCommand(backupSql, conn) { CommandTimeout = 600 };
                    await cmd.ExecuteNonQueryAsync();
                }

                var zipFilePath = Path.Combine(tempFolder, $"{_dbName}_backup.zip");
                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(bakFilePath, bakFileName);
                }

                return await File.ReadAllBytesAsync(zipFilePath);
            }
            finally
            {
                // Cleanup সবসময় হবে, error হলেও
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, recursive: true);
            }
        }
    }
}


