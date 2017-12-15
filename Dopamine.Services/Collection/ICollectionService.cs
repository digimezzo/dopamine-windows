using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.Services.Collection
{
    public interface ICollectionService
    {
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks);
        Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks);
        Task SetAlbumArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels, int delayMilliSeconds);
        Task RefreshArtworkAsync(ObservableCollection<AlbumViewModel> albumViewModels, List<long> albumsIds = null);
        Task MarkFolderAsync(Folder folder);
        event EventHandler CollectionChanged;
    }
}
