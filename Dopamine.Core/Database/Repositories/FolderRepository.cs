using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public abstract class FolderRepository : IFolderRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Properties
        public SQLiteConnectionFactory Factory => this.factory;
        #endregion

        #region Construction
        public FolderRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IFolderRepository
        public abstract Task<List<Folder>> GetFoldersAsync();

        public abstract Task<List<Tuple<long, string, long>>> GetPathsAsync();

        public abstract Task<AddFolderResult> AddFolderAsync(string path);

        public abstract Task<RemoveFolderResult> RemoveFolderAsync(string path);

        public abstract Task UpdateFoldersAsync(IList<Folder> folders);
        #endregion
    }
}
