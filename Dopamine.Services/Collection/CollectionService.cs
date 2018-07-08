using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Utils;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Entities;
using Dopamine.Services.Extensions;
using Dopamine.Services.Playback;
using Dopamine.Services.Utils;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Dopamine.Services.Collection
{
    public class CollectionService : ICollectionService
    {
        private ITrackRepository trackRepository;
        private IFolderRepository folderRepository;
        private ICacheService cacheService;
        private IPlaybackService playbackService;
        private IContainerProvider container;
        private List<Folder> markedFolders;
        private Timer saveMarkedFoldersTimer = new Timer(2000);

        public CollectionService(ITrackRepository trackRepository, IFolderRepository folderRepository, ICacheService cacheService, IPlaybackService playbackService, IContainerProvider container)
        {
            this.trackRepository = trackRepository;
            this.folderRepository = folderRepository;
            this.cacheService = cacheService;
            this.playbackService = playbackService;
            this.container = container;
            this.markedFolders = new List<Folder>();

            this.saveMarkedFoldersTimer.Elapsed += SaveMarkedFoldersTimer_Elapsed;
        }

        public event EventHandler CollectionChanged = delegate { };

        private async Task SaveMarkedFoldersAsync()
        {
            bool isCollectionChanged = false;

            try
            {
                isCollectionChanged = this.markedFolders.Count > 0;
                await this.folderRepository.UpdateFoldersAsync(this.markedFolders);
                this.markedFolders.Clear();
            }
            catch (Exception ex)
            {
                LogClient.Error("Error updating folders. Exception: {0}", ex.Message);
            }

            if (isCollectionChanged)
            {
                // Execute on Dispatcher as this will cause a refresh of the lists
                Application.Current.Dispatcher.Invoke(() => this.CollectionChanged(this, new EventArgs()));
            }
        }

        private async void SaveMarkedFoldersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await this.SaveMarkedFoldersAsync();
        }

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
                    // When the track is playing, the corresponding file is handled by the CSCore.
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

        public async Task MarkFolderAsync(Folder fol)
        {
            this.saveMarkedFoldersTimer.Stop();

            await Task.Run(() =>
            {
                try
                {
                    lock (this.markedFolders)
                    {
                        if (this.markedFolders.Contains(fol))
                        {
                            this.markedFolders[this.markedFolders.IndexOf(fol)].ShowInCollection = fol.ShowInCollection;
                        }
                        else
                        {
                            this.markedFolders.Add(fol);
                        }
                    }

                    this.saveMarkedFoldersTimer.Start();
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error marking folder with path='{0}'. Exception: {1}", fol.Path, ex.Message);
                }
            });
        }

        private async Task<IList<ArtistViewModel>> GetUniqueArtistsAsync(IList<string> artists)
        {
            IList<ArtistViewModel> uniqueArtists = new List<ArtistViewModel>();

            await Task.Run(() =>
            {
                foreach (string artist in artists)
                {
                    var newArtist = new ArtistViewModel(artist);

                    if (!uniqueArtists.Contains(newArtist))
                    {
                        uniqueArtists.Add(newArtist);
                    }
                }

                var unknownArtist = new ArtistViewModel(ResourceUtils.GetString("Language_Unknown_Artist"));

                if (!uniqueArtists.Contains(unknownArtist))
                {
                    uniqueArtists.Add(unknownArtist);
                }
            });

            return uniqueArtists;
        }

        private async Task<IList<GenreViewModel>> GetUniqueGenresAsync(IList<string> genres)
        {
            IList<GenreViewModel> uniqueGenres = new List<GenreViewModel>();

            await Task.Run(() =>
            {
                foreach (string genre in genres)
                {
                    var newGenre = new GenreViewModel(genre);

                    if (!uniqueGenres.Contains(newGenre))
                    {
                        uniqueGenres.Add(newGenre);
                    }
                }

                var unknownGenre = new GenreViewModel(ResourceUtils.GetString("Language_Unknown_Genre"));

                if (!uniqueGenres.Contains(unknownGenre))
                {
                    uniqueGenres.Add(unknownGenre);
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
                    var newAlbum = new AlbumViewModel(album, true);

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
            IList<GenreViewModel> orderedGenres = (await this.GetUniqueGenresAsync(genres)).OrderBy(g => g.GenreName).ToList();

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
                    IList<string> trackArtiss = await this.trackRepository.GetTrackArtistsAsync();
                    IList<string> albumArtists = await this.trackRepository.GetAlbumArtistsAsync();
                    ((List<string>)trackArtiss).AddRange(albumArtists);
                    artists = trackArtiss;
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

            IList<ArtistViewModel> orderedArtists = (await this.GetUniqueArtistsAsync(artists)).OrderBy(a => a.ArtistName).ToList();

            // Workaround to make sure the "#" GroupHeader is shown at the top of the list
            List<ArtistViewModel> tempArtistViewModels = new List<ArtistViewModel>();
            tempArtistViewModels.AddRange(orderedArtists.Where((avm) => avm.Header.Equals("#")));
            tempArtistViewModels.AddRange(orderedArtists.Where((avm) => !avm.Header.Equals("#")));

            return tempArtistViewModels;
        }

        public async Task<IList<AlbumViewModel>> GetAllAlbumsAsync()
        {
            IList<AlbumData> albums = await this.trackRepository.GetAlbumsAsync(null, null);

            return await this.GetUniqueAlbumsAsync(albums);
        }

        public async Task<IList<AlbumViewModel>> GetArtistAlbumsAsync(IList<string> selectedArtists)
        {
            IList<AlbumData> albums = await this.trackRepository.GetAlbumsAsync(selectedArtists.Select(x => x.Replace(ResourceUtils.GetString("Language_Unknown_Artist"), string.Empty)).ToList(), null);

            return await this.GetUniqueAlbumsAsync(albums);
        }

        public async Task<IList<AlbumViewModel>> GetGenreAlbumsAsync(IList<string> selectedGenres)
        {
            IList<AlbumData> albums = await this.trackRepository.GetAlbumsAsync(null, selectedGenres.Select(x => x.Replace(ResourceUtils.GetString("Language_Unknown_Genre"), string.Empty)).ToList());

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
                    case AlbumOrder.ByYear:
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
                        case AlbumOrder.ByYear:
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

        public async Task<IList<TrackViewModel>> GetArtistTracksAsync(IList<string> selectedArtists, TrackOrder trackOrder)
        {
            IList<Track> tracks = await this.trackRepository.GetArtistTracksAsync(selectedArtists);
            IList<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(await this.container.ResolveTrackViewModelsAsync(tracks), trackOrder);

            return orderedTracks;
        }

        public async Task<IList<TrackViewModel>> GetAlbumsTracksAsync(IList<string> selectedAlbumKeys, TrackOrder trackOrder)
        {
            IList<Track> tracks = await this.trackRepository.GetAlbumTracksAsync(selectedAlbumKeys);
            IList<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(await this.container.ResolveTrackViewModelsAsync(tracks), trackOrder);

            return orderedTracks;
        }

        public async Task<IList<TrackViewModel>> GetGenreTracksAsync(IList<string> selectedGenres, TrackOrder trackOrder)
        {
            IList<Track> tracks = await this.trackRepository.GetGenreTracksAsync(selectedGenres);
            IList<TrackViewModel> orderedTracks = await EntityUtils.OrderTracksAsync(await this.container.ResolveTrackViewModelsAsync(tracks), trackOrder);

            return orderedTracks;
        }
    }
}
