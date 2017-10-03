using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Helpers;
using Dopamine.Common.Presentation.ViewModels.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Collection
{
    public interface ICollectionService
    {
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks);
        Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks);
        Task SetAlbumArtworkAsync(ObservableCollection<AlbumViewModel> albumViewmodels, int delayMilliSeconds);
        Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels);
        Task MarkFolderAsync(Folder folder);
        Task SaveMarkedFoldersAsync();
        event EventHandler CollectionChanged;
    }
}
