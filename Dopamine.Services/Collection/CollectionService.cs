using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Utils;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Entities;
using Dopamine.Services.Playback;
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
        private List<Folder> markedFolders;
        private Timer saveMarkedFoldersTimer = new Timer(2000);

        public CollectionService(ITrackRepository trackRepository, IFolderRepository folderRepository, ICacheService cacheService, IPlaybackService playbackService)
        {
            this.trackRepository = trackRepository;
            this.folderRepository = folderRepository;
            this.cacheService = cacheService;
            this.playbackService = playbackService;
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

        public async Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks)
        {
            RemoveTracksResult result = await this.trackRepository.RemoveTracksAsync(selectedTracks);

            if (result == RemoveTracksResult.Success)
            {
                this.CollectionChanged(this, new EventArgs());
            }

            return result;
        }

        public async Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks)
        {
            var sendToRecycleBinResult = RemoveTracksResult.Success;
            var result = await this.trackRepository.RemoveTracksAsync(selectedTracks);

            if (result == RemoveTracksResult.Success)
            {
                // If result is Success: we can assume that all selected tracks were removed from the collection,
                // as this happens in a transaction in trackRepository. If removing 1 or more tracks fails, the
                // transaction is rolled back and no tracks are removed.
                foreach (var track in selectedTracks)
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

        private async Task<IList<ArtistViewModel>> GetUniqueArtistsAsync(IList<string> artistNames)
        {
            IList<ArtistViewModel> uniqueArtists = new List<ArtistViewModel>();

            await Task.Run(() =>
            {
                foreach (string artistName in artistNames)
                {
                    var newArtist = new ArtistViewModel(artistName);

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

        private async Task<IList<GenreViewModel>> GetUniqueGenresAsync(IList<string> genreNames)
        {
            IList<GenreViewModel> uniqueGenres = new List<GenreViewModel>();

            await Task.Run(() =>
            {
                foreach (string genreName in genreNames)
                {
                    var newGenre = new GenreViewModel(genreName);

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

        public async Task<IList<GenreViewModel>> GetAllGenresAsync()
        {
            IList<string> genreNames = await this.trackRepository.GetAllGenreNamesAsync();
            IList<GenreViewModel> orderedGenres = (await this.GetUniqueGenresAsync(genreNames)).OrderBy(g => g.GenreName).ToList();

            // Workaround to make sure the "#" GroupHeader is shown at the top of the list
            List<GenreViewModel> tempGenreViewModels = new List<GenreViewModel>();
            tempGenreViewModels.AddRange(orderedGenres.Where((gvm) => gvm.Header.Equals("#")));
            tempGenreViewModels.AddRange(orderedGenres.Where((gvm) => !gvm.Header.Equals("#")));

            return tempGenreViewModels;
        }

        public async Task<IList<ArtistViewModel>> GetAllArtistsAsync(ArtistType artistType)
        {
            IList<string> artistNames = null;

            switch (artistType)
            {
                case ArtistType.All:
                    IList<string> trackArtistNames = await this.trackRepository.GetAllTrackArtistNamesAsync();
                    IList<string> albumArtistNames = await this.trackRepository.GetAllAlbumArtistNamesAsync();
                    ((List<string>)trackArtistNames).AddRange(albumArtistNames);
                    artistNames = trackArtistNames;
                    break;
                case ArtistType.Track:
                    artistNames = await this.trackRepository.GetAllTrackArtistNamesAsync();
                    break;
                case ArtistType.Album:
                    artistNames = await this.trackRepository.GetAllAlbumArtistNamesAsync();
                    break;
                default:
                    // Can't happen
                    break;
            }

            IList<ArtistViewModel> orderedArtists = (await this.GetUniqueArtistsAsync(artistNames)).OrderBy(a => a.ArtistName).ToList();

            // Workaround to make sure the "#" GroupHeader is shown at the top of the list
            List<ArtistViewModel> tempArtistViewModels = new List<ArtistViewModel>();
            tempArtistViewModels.AddRange(orderedArtists.Where((avm) => avm.Header.Equals("#")));
            tempArtistViewModels.AddRange(orderedArtists.Where((avm) => !avm.Header.Equals("#")));

            return tempArtistViewModels;
        }

        public async Task<IList<AlbumViewModel>> GetAllAlbumsAsync()
        {
            IList<AlbumValues> albumValuesList = await this.trackRepository.GetAllAlbumValuesAsync();
            IList<AlbumViewModel> albums = new List<AlbumViewModel>();

            foreach (AlbumValues albumValues in albumValuesList)
            {
                var album = new AlbumViewModel(
                    albumValues.AlbumTitle,
                    albumValues.AlbumArtists,
                    albumValues.Year,
                    albumValues.AlbumKey,
                    albumValues.DateAdded,
                    albumValues.DateFileCreated);

                if (!albums.Contains(album))
                {
                    albums.Add(album);
                }
            }

            return albums;
        }

        public async Task<IList<AlbumViewModel>> GetArtistAlbumsAsync(IList<string> selectedArtists)
        {
            // throw new NotImplementedException();
            return new List<AlbumViewModel>();
        }

        public async Task<IList<AlbumViewModel>> GetGenreAlbumsAsync(IList<string> selectedGenres)
        {
            // throw new NotImplementedException();
            return new List<AlbumViewModel>();
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
    }
}
