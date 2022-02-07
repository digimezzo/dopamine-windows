using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Utils;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Entities;
using Dopamine.Services.Playback;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Collection
{
    public class CollectionService : ICollectionService
    {
        private ITrackRepository trackRepository;
        private IFolderRepository folderRepository;
        private ICacheService cacheService;
        private IPlaybackService playbackService;
        private IContainerProvider container;

        public CollectionService(ITrackRepository trackRepository, IFolderRepository folderRepository, ICacheService cacheService, IPlaybackService playbackService, IContainerProvider container)
        {
            this.trackRepository = trackRepository;
            this.folderRepository = folderRepository;
            this.cacheService = cacheService;
            this.playbackService = playbackService;
            this.container = container;
        }

        public event EventHandler CollectionChanged = delegate { };

        public async Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<TrackViewModel> selectedTracks)
        {
            RemoveTracksResult result = await this.trackRepository.RemoveTracksAsync(selectedTracks.Select(t => t.Track).ToList());

            if (result == RemoveTracksResult.Success)
            {
                this.CollectionChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<TrackViewModel> selectedTracks)
        {
            var sendToRecycleBinResult = RemoveTracksResult.Success;
            var result = await this.trackRepository.RemoveTracksAsync(selectedTracks.Select(t => t.Track).ToList());

            if (result == RemoveTracksResult.Success)
            {
                // If result is Success: we can assume that all selected tracks were removed from the collection,
                // as this happens in a transaction in trackRepository. If removing 1 or more tracks fails, the
                // transaction is rolled back and no tracks are removed.
                foreach (TrackViewModel track in selectedTracks)
                {
                    // When the track is playing, the corresponding file is handled by IPlayer.
                    // To delete the file properly, PlaybackService must release this handle.
                    await this.playbackService.StopIfPlayingAsync(track);

                    try
                    {
                        // Delete file from disk
                        FileUtils.SendToRecycleBinSilent(track.Path);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error($"Error while removing track '{track.TrackTitle}' from disk. Exception: {ex.Message}");
                        sendToRecycleBinResult = RemoveTracksResult.Error;
                    }
                }

                this.CollectionChanged(this, new EventArgs());
            }

            if (sendToRecycleBinResult == RemoveTracksResult.Success && result == RemoveTracksResult.Success)
                return RemoveTracksResult.Success;
            return RemoveTracksResult.Error;
        }

        private async Task<IList<ArtistViewModel>> GetUniqueArtistsAsync(IList<string> artists)
        {
            IList<ArtistViewModel> uniqueArtists = new List<ArtistViewModel>();

            await Task.Run(() =>
            {
                bool hasUnknownArtists = false;

                foreach (string artist in artists)
                {
                    if (!string.IsNullOrEmpty(artist))
                    {
                        var newArtist = new ArtistViewModel(artist);

                        if (!uniqueArtists.Contains(newArtist))
                        {
                            uniqueArtists.Add(newArtist);
                        }
                    }
                    else
                    {
                        hasUnknownArtists = true;
                    }
                }

                if (hasUnknownArtists)
                {
                    var unknownArtist = new ArtistViewModel(ResourceUtils.GetString("Language_Unknown_Artist"));

                    if (!uniqueArtists.Contains(unknownArtist))
                    {
                        uniqueArtists.Add(unknownArtist);
                    }
                }
            });

            return uniqueArtists;
        }

        private async Task<IList<GenreViewModel>> GetUniqueGenresAsync(IList<string> genres)
        {
            IList<GenreViewModel> uniqueGenres = new List<GenreViewModel>();

            await Task.Run(() =>
            {
                bool hasUnknownGenres = false;

                foreach (string genre in genres)
                {
                    if (!string.IsNullOrEmpty(genre))
                    {
                        var newGenre = new GenreViewModel(genre);

                        if (!uniqueGenres.Contains(newGenre))
                        {
                            uniqueGenres.Add(newGenre);
                        }
                    }
                    else
                    {
                        hasUnknownGenres = true;
                    }
                }

                if (hasUnknownGenres)
                {
                    var unknownGenre = new GenreViewModel(ResourceUtils.GetString("Language_Unknown_Genre"));

                    if (!uniqueGenres.Contains(unknownGenre))
                    {
                        uniqueGenres.Add(unknownGenre);
                    }
                }
            });

            return uniqueGenres;
        }

        private async Task<IList<AlbumViewModel>> GetUniqueAlbumsAsync(IList<AlbumData> albums)
        {
            IList<AlbumViewModel> uniqueAlbums = new List<AlbumViewModel>();

            await Task.Run(() =>
            {
                foreach (AlbumData album in albums)
                {
                    var newAlbum = new AlbumViewModel(album);

                    if (!uniqueAlbums.Contains(newAlbum))
                    {
                        uniqueAlbums.Add(newAlbum);
                    }
                }
            });

            return uniqueAlbums;
        }


        public async Task<IList<GenreViewModel>> GetAllGenresAsync()
        {
            IList<string> genres = await this.trackRepository.GetGenresAsync();
            IList<GenreViewModel> orderedGenres = (await this.GetUniqueGenresAsync(genres)).OrderBy(g => FormatUtils.GetSortableString(g.GenreName, true)).ToList();

            // Workaround to make sure the "#" GroupHeader is shown at the top of the list
            List<GenreViewModel> tempGenreViewModels = new List<GenreViewModel>();
            tempGenreViewModels.AddRange(orderedGenres.Where((gvm) => gvm.Header.Equals("#")));
            tempGenreViewModels.AddRange(orderedGenres.Where((gvm) => !gvm.Header.Equals("#")));

            return tempGenreViewModels;
        }

        public async Task<IList<ArtistViewModel>> GetAllArtistsAsync(ArtistType artistType)
        {
            IList<string> artists = null;

            switch (artistType)
            {
                case ArtistType.All:
                    IList<string> trackArtists = await this.trackRepository.GetTrackArtistsAsync();
                    IList<string> albumArtists = await this.trackRepository.GetAlbumArtistsAsync();
                    ((List<string>)trackArtists).AddRange(albumArtists);
                    artists = trackArtists;
                    break;
                case ArtistType.Track:
                    artists = await this.trackRepository.GetTrackArtistsAsync();
                    break;
                case ArtistType.Album:
                    artists = await this.trackRepository.GetAlbumArtistsAsync();
                    break;
                default:
                    // Can't happen	
                    break;
            }

            IList<ArtistViewModel> orderedArtists = (await this.GetUniqueArtistsAsync(artists)).OrderBy(a => FormatUtils.GetSortableString(a.ArtistName, true)).ToList();

            // Workaround to make sure the "#" GroupHeader is shown at the top of the list
            List<ArtistViewModel> tempArtistViewModels = new List<ArtistViewModel>();
            tempArtistViewModels.AddRange(orderedArtists.Where((avm) => avm.Header.Equals("#")));
            tempArtistViewModels.AddRange(orderedArtists.Where((avm) => !avm.Header.Equals("#")));

            return tempArtistViewModels;
        }

        public async Task<IList<AlbumViewModel>> GetAllAlbumsAsync()
        {
            IList<AlbumData> albums = await this.trackRepository.GetAllAlbumDataAsync();

            return await this.GetUniqueAlbumsAsync(albums);
        }

        public async Task<IList<AlbumViewModel>> GetArtistAlbumsAsync(IList<string> selectedArtists, ArtistType artistType)
        {
            IList<AlbumData> albums = await this.trackRepository.GetArtistAlbumDataAsync(selectedArtists.Select(x => x.Replace(ResourceUtils.GetString("Language_Unknown_Artist"), string.Empty)).ToList(), artistType);

            return await this.GetUniqueAlbumsAsync(albums);
        }

        public async Task<IList<AlbumViewModel>> GetGenreAlbumsAsync(IList<string> selectedGenres)
        {
            IList<AlbumData> albums = await this.trackRepository.GetGenreAlbumDataAsync(selectedGenres.Select(x => x.Replace(ResourceUtils.GetString("Language_Unknown_Genre"), string.Empty)).ToList());

            return await this.GetUniqueAlbumsAsync(albums);
        }

        public async Task<IList<AlbumViewModel>> OrderAlbumsAsync(IList<AlbumViewModel> albums, AlbumOrder albumOrder)
        {
            var orderedAlbums = new List<AlbumViewModel>();

            await Task.Run(() =>
            {
                switch (albumOrder)
                {
                    case AlbumOrder.Alphabetical:
                        orderedAlbums = albums.OrderBy((a) => FormatUtils.GetSortableString(a.AlbumTitle)).ToList();
                        break;
                    case AlbumOrder.ByDateAdded:
                        orderedAlbums = albums.OrderByDescending((a) => a.DateAdded).ToList();
                        break;
                    case AlbumOrder.ByDateCreated:
                        orderedAlbums = albums.OrderByDescending((a) => a.DateFileCreated).ToList();
                        break;
                    case AlbumOrder.ByAlbumArtist:
                        orderedAlbums = albums.OrderBy((a) => FormatUtils.GetSortableString(a.AlbumArtist, true)).ToList();
                        break;
                    case AlbumOrder.ByYearAscending:
                        orderedAlbums = albums.OrderBy((a) => a.SortYear).ToList();
                        break;
                    case AlbumOrder.ByYearDescending:
                        orderedAlbums = albums.OrderByDescending((a) => a.SortYear).ToList();
                        break;
                    default:
                        // Alphabetical
                        orderedAlbums = albums.OrderBy((a) => FormatUtils.GetSortableString(a.AlbumTitle)).ToList();
                        break;
                }

                foreach (AlbumViewModel alb in orderedAlbums)
                {
                    string mainHeader = alb.AlbumTitle;
                    string subHeader = alb.AlbumArtist;

                    switch (albumOrder)
                    {
                        case AlbumOrder.ByAlbumArtist:
                            mainHeader = alb.AlbumArtist;
                            subHeader = alb.AlbumTitle;
                            break;
                        case AlbumOrder.ByYearAscending:
                        case AlbumOrder.ByYearDescending:
                            mainHeader = alb.Year;
                            subHeader = alb.AlbumTitle;
                            break;
                        case AlbumOrder.Alphabetical:
                        case AlbumOrder.ByDateAdded:
                        case AlbumOrder.ByDateCreated:
                        default:
                            // Do nothing
                            break;
                    }

                    alb.MainHeader = mainHeader;
                    alb.SubHeader = subHeader;
                }
            });

            return orderedAlbums;
        }
    }
}
