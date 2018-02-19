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
                IEnumerable<String> allFilenames = GetAllFiles(directory, "*.*");

                foreach (string filename in allFilenames)
                {
                    try
                    {
                        // Only add the file if they have a valid extension
                        if (validExtensions.Contains(Path.GetExtension(filename.ToLower())))
                        {
                            folderPaths.Add(new FolderPathInfo(folderId, filename, FileUtils.DateModifiedTicks(filename)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Error occurred while getting folder path for file '{0}'. Exception: {1}", filename, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Unexpected error occurred while getting folder paths. Exception: {0}", ex.Message);
            }

            return folderPaths;
        }

        /// <summary>
        /// Recursively gets all files in a directory without failing 
        /// the complete operation when a directory inaccessible.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        private static IEnumerable<String> GetAllFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern).Union(
                Directory.EnumerateDirectories(path).SelectMany(d =>
                {
                    try
                    {
                        return GetAllFiles(d, searchPattern);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        return Enumerable.Empty<String>();
                    }
                }));
        }
    }
}
