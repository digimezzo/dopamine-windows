using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Dopamine.UWP.IO
{
    public static class FileOperations
    {
        public async static Task<long> GetDateModifiedAsync(StorageFile file)
        {
            return (await file.GetBasicPropertiesAsync()).DateModified.Ticks;
        }

        public async static Task<long> GetFileSizeAsync(StorageFile file)
        {
            return (long)(await file.GetBasicPropertiesAsync()).Size;
        }

        public static long GetDateCreated(StorageFile file)
        {
            return file.DateCreated.UtcTicks;
        }

        public static string GetFileName(StorageFile file)
        {
            return file.DisplayName;
        }
    }
}
