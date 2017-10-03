using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface IFolderRepository
    {
        Task<List<Folder>> GetFoldersAsync();
        Task<List<Tuple<long, string, long>>> GetPathsAsync();
        Task<AddFolderResult> AddFolderAsync(string path);
        Task<RemoveFolderResult> RemoveFolderAsync(string path);
        Task UpdateFoldersAsync(IList<Folder> folders);
    }
}
