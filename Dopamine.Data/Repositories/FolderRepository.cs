using Digimezzo.Utilities.Log;
using Dopamine.Core.Extensions;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private ISQLiteConnectionFactory factory;

        public FolderRepository(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

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

        public async Task<RemoveFolderResult> RemoveFolderAsync(long folderId)
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
                            conn.Execute($"DELETE FROM Folder WHERE FolderID={folderId};");
                            conn.Execute($"DELETE FROM FolderTrack WHERE FolderID={folderId};");

                            LogClient.Info("Removed the Folder with FolderID={0}", folderId);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Could not remove the Folder with FolderID={0}. Exception: {1}", folderId, ex.Message);
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
    }
}
