using Dopamine.Data;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Collection
{
    public interface ICollectionService
    {
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks);

        Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks);

        Task MarkFolderAsync(Folder folder);

        Task<IList<string>> GetAllTrackArtists();

        Task<IList<string>> GetAllAlbumArtists();

        Task<IList<string>> GetAllArtists();

        Task<IList<string>> GetAllGenres();

        event EventHandler CollectionChanged;
    }
}
