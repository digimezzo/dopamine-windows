using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Logging;
using Dopamine.UWP.Helpers;
using Dopamine.UWP.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Dopamine.UWP.Database.Repositories
{
    public class FolderRepository : Core.Database.Repositories.FolderRepository
    {
        #region Construction
        public FolderRepository(ISQLiteConnectionFactory factory) : base(factory)
        {
        }
        #endregion

        #region Overrides
        public override async Task<List<Tuple<long, string, long>>> GetPathsAsync()
        {
            var diskPaths = new List<Tuple<long, string, long>>();

            var iterator = new RecursiveFileIterator();

            List<StorageFile> files = await iterator.GetFiles(KnownFolders.MusicLibrary, FileFormats.SupportedMediaExtensions);

            foreach (var file in files)
            {
                // FolderId is not used in UWP. Just fill in 0.
                diskPaths.Add(new Tuple<long, string, long>(0, file.Path, await FileOperations.GetDateModifiedAsync(file)));
            }

            foreach (var ex in iterator.Exceptions)
            {
                CoreLogger.Current.Error("Error while recursively getting files/folders. Exception: {0}", ex.ToString());
            }

            return diskPaths;
        }

        public override Task<AddFolderResult> AddFolderAsync(string path)
        {
            throw new NotImplementedException();
        }

        public override Task<List<Folder>> GetFoldersAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<RemoveFolderResult> RemoveFolderAsync(string path)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateFoldersAsync(IList<Folder> folders)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
