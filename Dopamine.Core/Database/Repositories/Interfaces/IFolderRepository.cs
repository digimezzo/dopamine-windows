using Dopamine.Core.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories.Interfaces
{
    public interface IFolderRepository
    {
        Task<List<Folder>> GetFoldersAsync();
        Task<List<Tuple<long, string, long>>> GetPathsAsync();
        Task<AddFolderResult> AddFolderAsync(Folder folder);
        Task<RemoveFolderResult> RemoveFolderAsync(string path);
        Task UpdateFoldersAsync(IList<Folder> folders);
    }
}
