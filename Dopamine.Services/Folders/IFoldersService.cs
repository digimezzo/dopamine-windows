using Dopamine.Data;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Folders
{
    public interface IFoldersService
    {
        event EventHandler FoldersChanged;

        Task MarkFolderAsync(FolderViewModel folder);

        Task<IList<FolderViewModel>> GetFoldersAsync();

        Task<AddFolderResult> AddFolderAsync(string path);

        Task<RemoveFolderResult> RemoveFolderAsync(long folderId);

        Task<FolderViewModel> GetSelectedFolderAsync();

        Task<IList<SubfolderViewModel>> GetSubfoldersAsync(FolderViewModel selectedRootFolder, SubfolderViewModel selectedSubfolder);

        Task<IList<SubfolderBreadCrumb>> GetSubfolderBreadCrumbsAsync(FolderViewModel selectedRootFolder, string path);
    }
}
