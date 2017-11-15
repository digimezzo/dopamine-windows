using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dopamine.Common.IO
{
    public sealed class FileOperations
    {
        public static List<string> DirectoryRecursiveGetValidFiles(string directory, string[] validExtensions)
        {
            try
            {
                var di = new DirectoryInfo(directory);
                IEnumerable<FileInfo> fi = di.GetFiles("*.*", SearchOption.AllDirectories);

                var paths = new List<string>();

                // Only add the file if they have a valid extension
                paths.AddRange(fi.Where(f => validExtensions.Contains(Path.GetExtension(f.FullName.ToLower()))).Select(f => f.FullName).ToList());

                return paths;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
