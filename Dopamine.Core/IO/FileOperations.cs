using Digimezzo.Foundation.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Dopamine.Core.IO
{
    public sealed class FileOperations
    {
        public static Task<List<FolderPathInfo>> GetValidFolderPathsAsync(
            long folderId,
            string directory,
            string[] validExtensions,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return GetValidFolderPaths(folderId, directory, validExtensions, cancellationToken);
            });
        }

        private static List<FolderPathInfo> GetValidFolderPaths(
            long folderId,
            string directory,
            string[] validExtensions,
            CancellationToken cancellationToken)
        {
            LogClient.Info("Get paths of directory {0}", directory);

            var folderPaths = new List<FolderPathInfo>();
            var validExtensionSet = new HashSet<string>(validExtensions);

            var sw = Stopwatch.StartNew();

            try
            {
                var files = new List<FileInfo>();
                var exceptions = new ConcurrentQueue<Exception>();

                var sw2 = Stopwatch.StartNew();

                TryDirectoryRecursiveGetFiles(directory, files, exceptions, cancellationToken);

                sw2.Stop();

                LogClient.Info("Retrieved {0} files from {1} ({2} ms)", files.Count, directory, sw2.ElapsedMilliseconds);

                foreach (Exception ex in exceptions)
                {
                    LogClient.Error("Error occurred while getting files recursively. Exception: {0}", ex.Message);
                }

                folderPaths.Capacity = files.Count;

                Parallel.ForEach(
                    files,
                    new ParallelOptions { CancellationToken = cancellationToken },
                    file =>
                {
                    try
                    {
                        var extension = file.Extension.ToLower();

                        // Only add the file if they have a valid extension
                        if (validExtensionSet.Contains(extension))
                        {
                            var dateModifiedTicks = file.LastWriteTime.Ticks;

                            lock (folderPaths)
                            {
                                folderPaths.Add(new FolderPathInfo(folderId, file.FullName, dateModifiedTicks));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Error occurred while getting folder path for file '{0}'. Exception: {1}", file, ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Unexpected error occurred while getting folder paths. Exception: {0}", ex.Message);
            }

            sw.Stop();

            LogClient.Info("Get paths of directory {0} finished ({1} ms)", directory, sw.ElapsedMilliseconds);

            return folderPaths;
        }

        private static void TryDirectoryRecursiveGetFiles(            
            string path,
            List<FileInfo> files,
            ConcurrentQueue<Exception> exceptions,
            CancellationToken cancellationToken)
        {
            // Process the list of files found in the directory.
            try
            {
                var fileEntries = new DirectoryInfo(path).GetFiles();

                lock (files)
                {
                    files.AddRange(fileEntries);
                }
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }

            // Recurse into subdirectories of this directory. 
            try
            {
                var subdirectoryEntries = Directory.GetDirectories(path);

                Parallel.ForEach(
                    subdirectoryEntries,
                    new ParallelOptions { CancellationToken = cancellationToken },
                    subdirectory =>
                {
                    try
                    {
                        TryDirectoryRecursiveGetFiles(subdirectory, files, exceptions, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
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
