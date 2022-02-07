using Dopamine.Data;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Collection
{
    public interface ICollectionService
    {
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<TrackViewModel> selectedTracks);

        Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<TrackViewModel> selectedTracks);

        Task<IList<ArtistViewModel>> GetAllArtistsAsync(ArtistType artistType);

        Task<IList<GenreViewModel>> GetAllGenresAsync();

        Task<IList<AlbumViewModel>> GetAllAlbumsAsync();

        Task<IList<AlbumViewModel>> GetArtistAlbumsAsync(IList<string> selectedArtists, ArtistType artistType);

        Task<IList<AlbumViewModel>> GetGenreAlbumsAsync(IList<string> selectedGenres);

        Task<IList<AlbumViewModel>> OrderAlbumsAsync(IList<AlbumViewModel> albums, AlbumOrder albumOrder);

        event EventHandler CollectionChanged;
       
    }
}
