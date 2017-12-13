using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dopamine.Core.IO
{
    public sealed class FileOperations
    {
        public static List<FolderPathInfo> GetValidFolderPaths(long folderId, string directory, string[] validExtensions, SearchOption searchOption)
        {
            var folderPaths = new List<FolderPathInfo>();

            try
            {
                var di = new DirectoryInfo(directory);
                IEnumerable<FileInfo> fi = di.GetFiles("*.*", searchOption);

                foreach (FileInfo f in fi)
                {
                    try
                    {
                        // Only add the file if they have a valid extension
                        if (validExtensions.Contains(Path.GetExtension(f.FullName.ToLower())))
                        {
                            folderPaths.Add(new FolderPathInfo(folderId, f.FullName, FileUtils.DateModifiedTicks(f.FullName)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Error occurred while getting folder path for file '{0}'. Exception: {1}", f.FullName, ex.Message);
                    }   
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Unexpected error occurred while getting folder paths. Exception: {0}", ex.Message);
            }

            return folderPaths;
        }
    }
}
