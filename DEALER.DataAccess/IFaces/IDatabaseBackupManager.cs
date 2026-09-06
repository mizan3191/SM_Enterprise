using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEALER.DataAccess
{
    public interface IDatabaseBackupManager
    {
        Task<byte[]> CreateBackupZipAsync();
    }
}
