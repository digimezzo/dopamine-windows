using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dopamine.Core.IO
{
    public sealed class FileOperations
    {
        public static List<FolderPathInfo> GetValidFolderPaths(long folderId, string directory, string[] validExtensions)
        {
            var folderPaths = new List<FolderPathInfo>();

            try
            {
                var files = new List<string>();
                var exceptions = new ConcurrentQueue<Exception>();

                TryDirectoryRecursiveGetFiles(directory, files, exceptions);

                foreach (Exception ex in exceptions)
                {
                    LogClient.Error("Error occurred while getting files recursively. Exception: {0}", ex.Message);
                }

                foreach (string file in files)
                {
                    try
                    {
                        // Only add the file if they have a valid extension
                        if (validExtensions.Contains(Path.GetExtension(file.ToLower())))
                        {
                            folderPaths.Add(new FolderPathInfo(folderId, file, FileUtils.DateModifiedTicks(file)));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Error occurred while getting folder path for file '{0}'. Exception: {1}", file, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Unexpected error occurred while getting folder paths. Exception: {0}", ex.Message);
            }

            return folderPaths;
        }

        private static void TryDirectoryRecursiveGetFiles(string path, List<String> files, ConcurrentQueue<Exception> exceptions)
        {
            try
            {
                // Process the list of files found in the directory.
                string[] fileEntries = null;

                try
                {
                    fileEntries = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }

                if (fileEntries != null && fileEntries.Count() > 0)
                {
                    foreach (string fileName in fileEntries)
                    {
                        try
                        {
                            files.Add(fileName);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Enqueue(ex);
                        }
                    }
                }

                // Recurse into subdirectories of this directory. 
                string[] subdirectoryEntries = null;

                try
                {
                    subdirectoryEntries = Directory.GetDirectories(path);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }

                if (subdirectoryEntries != null && subdirectoryEntries.Count() > 0)
                {

                    foreach (string subdirectory in subdirectoryEntries)
                    {
                        try
                        {
                            TryDirectoryRecursiveGetFiles(subdirectory, files, exceptions);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Enqueue(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
        }

        public static bool IsDirectoryContentAccessible(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return false;
            }

            try
            {
                var watcher = new FileSystemWatcher(directoryPath) { EnableRaisingEvents = true, IncludeSubdirectories = true };
                watcher.Dispose();
                watcher = null;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
