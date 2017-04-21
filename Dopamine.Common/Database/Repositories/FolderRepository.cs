using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Extensions;
using Dopamine.Common.IO;
using Digimezzo.Utilities.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public FolderRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IFolderRepository
        public async Task<AddFolderResult> AddFolderAsync(string path)
        {
            AddFolderResult result = AddFolderResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            if (!conn.Table<Folder>().Select((f) => f).ToList().Select((f) => f.SafePath).Contains(path.ToSafePath()))
                            {
                                conn.Insert(new Folder { Path = path, SafePath = path.ToSafePath(), ShowInCollection = 1 });
                                LogClient.Info("Added the Folder {0}", path);
                            }
                            else
                            {
                                LogClient.Info("Didn't add the Folder {0} because it is already in the database", path);
                                result = AddFolderResult.Duplicate;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not add the Folder {0}. Exception: {1}", path, ex.Message);
                            result = AddFolderResult.Error;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<RemoveFolderResult> RemoveFolderAsync(string path)
        {
            RemoveFolderResult result = RemoveFolderResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            var obsoleteFolder = conn.Table<Folder>().Select((f) => f).ToList().Where((f) => f.SafePath.Equals(path.ToSafePath())).Select((f) => f).FirstOrDefault();

                            if (obsoleteFolder != null)
                            {
                                conn.Delete(obsoleteFolder);
                                LogClient.Info("Removed the Folder {0}", path);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not remove the Folder {0}. Exception: {1}", path, ex.Message);
                            result = RemoveFolderResult.Error;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<List<Folder>> GetFoldersAsync()
        {
            var allFolders = new List<Folder>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            allFolders = conn.Table<Folder>().Select((s) => s).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not get all the Folders. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }

            });

            return allFolders;
        }

        public async Task<List<Tuple<long, string, long>>> GetPathsAsync()
        {
            var diskPaths = new Dictionary<string, Tuple<long, string, long>>();
            List<Folder> folders = await this.GetFoldersAsync();

            await Task.Run(() =>
            {
                // Recursively get all the files in the collection folders
                foreach (Folder fol in folders)
                {
                    if (Directory.Exists(fol.Path))
                    {
                        var paths = new List<string>();

                        try
                        {
                            // Create a queue to hold exceptions that have occurred while scanning the directory tree
                            var recurseExceptions = new ConcurrentQueue<Exception>();

                            // Get all audio files recursively
                            FileOperations.TryDirectoryRecursiveGetFiles(fol.Path, paths, FileFormats.SupportedMediaExtensions, recurseExceptions);

                            if (recurseExceptions.Count > 0)
                            {
                                foreach (Exception recurseException in recurseExceptions)
                                {
                                    LogClient.Error("Error while recursively getting files/folders. Exception: {0}", recurseException.ToString());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not iterate through the subdirectories of folder '{0}'. Exception: {1}", fol.Path, ex.Message);
                        }

                        foreach (string path in paths)
                        {
                            try
                            {
                                // Avoid adding duplicate paths
                                if (!diskPaths.Keys.Contains(path))
                                {
                                    diskPaths.Add(path, new Tuple<long, string, long>(fol.FolderID, path, FileUtils.DateModifiedTicks(path)));
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("Could not add path '{0}' to the list of folder paths, while processing folder '{1}'. Exception: {2}", path, fol.Path, ex.Message);
                            }
                        }
                    }
                }
            });

            return diskPaths.Values.ToList();
        }

        public async Task UpdateFoldersAsync(IList<Folder> folders)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            foreach (Folder fol in folders)
                            {
                                var dbFolder = conn.Table<Folder>().Select((f) => f).Where((f) => f.SafePath.Equals(fol.SafePath)).FirstOrDefault();

                                if (dbFolder != null)
                                {
                                    dbFolder.ShowInCollection = fol.ShowInCollection;
                                    conn.Update(dbFolder);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not update the Folders. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }
        #endregion
    }
}
