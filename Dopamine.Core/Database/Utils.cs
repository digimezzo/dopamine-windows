using Dopamine.Core.Database.Entities;
using Dopamine.Core.Extensions;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Database
{
    public sealed class Utils
    {
        public static async Task InitializeEntityFrameworkMetaDataAsync()
        {
            try
            {
                if (!DbCreator.DatabaseExists()) return;

                // Small dummy query to load EntityFramework Metadata
                await Task.Run(() =>
                {
                    using (var db = new DopamineContext())
                    {
                        db.Configurations.Select((s) => s);
                    }
                });
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Info("A problem occurred while preloading entity Framework Metadata. Exception: {0}", ex.Message);
            }
        }

        public static bool FilterAlbums(Album album, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (album.AlbumTitle == null) album.AlbumTitle = string.Empty;
            if (album.AlbumArtist == null) album.AlbumArtist = string.Empty;
            if (album.Year == null) album.Year = 0;

            return pieces.All(s => album.AlbumTitle.ToLower().Contains(s.ToLower()) | album.AlbumArtist.ToLower().Contains(s.ToLower()) | album.Year.ToString().ToLower().Contains(s.ToLower()));
        }

        public static bool FilterArtists(Artist artist, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (artist.ArtistName == null) artist.ArtistName = string.Empty;

            return pieces.All(s => artist.ArtistName.ToLower().Contains(s.ToLower()));
        }

        public static bool FilterGenres(Genre genre, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (genre.GenreName == null)
                genre.GenreName = string.Empty;

            return pieces.All(s => genre.GenreName.ToLower().Contains(s.ToLower()));
        }

        public static bool FilterTracks(TrackInfo trackInfo, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (trackInfo.Track.TrackTitle == null) trackInfo.Track.TrackTitle = string.Empty;
            if (trackInfo.Artist.ArtistName == null) trackInfo.Artist.ArtistName = string.Empty;
            if (trackInfo.Album.AlbumTitle == null) trackInfo.Album.AlbumTitle = string.Empty;
            if (trackInfo.Track.FileName == null) trackInfo.Track.FileName = string.Empty;
            if (trackInfo.Track.Year == null) trackInfo.Track.Year = 0;

            return pieces.All(s => trackInfo.Track.TrackTitle.ToLower().Contains(s.ToLower()) | trackInfo.Artist.ArtistName.ToLower().Contains(s.ToLower()) | trackInfo.Album.AlbumTitle.ToLower().Contains(s.ToLower()) | trackInfo.Track.FileName.ToLower().Contains(s.ToLower()) | trackInfo.Track.Year.ToString().Contains(s.ToLower()));
        }

        public static string GetSortableString(string originalString, bool removePrefix = false)
        {
            if (string.IsNullOrEmpty(originalString)) return string.Empty;

            string returnString = originalString.ToLower().Trim();

            if (removePrefix)
            {
                try
                {
                    returnString = returnString.TrimStart("the ").Trim();
                }
                catch (Exception)
                {
                    // Swallow
                }
            }

            return returnString;
        }

        public static async Task<List<Album>> OrderAlbumsAsync(IList<Album> albums, AlbumOrder albumOrder)
        {
            var orderedAlbums = new List<Album>();

            await Task.Run(() =>
            {
                switch (albumOrder)
                {
                    case AlbumOrder.Alphabetical:
                        orderedAlbums = albums.OrderBy(a => GetSortableString(a.AlbumTitle)).ToList();
                        break;
                    case AlbumOrder.ByDateAdded:
                        orderedAlbums = albums.OrderByDescending(a => a.DateAdded).ToList();
                        break;
                    case AlbumOrder.ByAlbumArtist:
                        orderedAlbums = albums.OrderBy(a => GetSortableString(a.AlbumArtist, true)).ToList();
                        break;
                    case AlbumOrder.ByYear:
                        orderedAlbums = albums.OrderByDescending(a => a.Year != null ? a.Year : 0).ToList();
                        break;
                    default:
                        // Alphabetical
                        orderedAlbums = albums.OrderBy(a => GetSortableString(a.AlbumTitle)).ToList();
                        break;
                }
            });

            return orderedAlbums;
        }

        public static async Task<List<TrackInfo>> OrderTracksAsync(IList<TrackInfo> tracks, TrackOrder trackOrder)
        {
            var orderedTracks = new List<TrackInfo>();

            await Task.Run(() =>
            {
                switch (trackOrder)
                {
                    case TrackOrder.Alphabetical:
                        orderedTracks = tracks.OrderBy(ti => !string.IsNullOrEmpty(GetSortableString(ti.Track.TrackTitle)) ? GetSortableString(ti.Track.TrackTitle) : GetSortableString(ti.Track.FileName)).ToList();
                        break;
                    case TrackOrder.ByAlbum:
                        orderedTracks = tracks.OrderBy(ti => GetSortableString(ti.Album.AlbumTitle)).ThenBy(ti => ti.Track.DiscNumber > 0 ? ti.Track.DiscNumber : 1).ThenBy(ti => ti.Track.TrackNumber).ToList();
                        break;
                    case TrackOrder.ByFileName:
                        orderedTracks = tracks.OrderBy(ti => GetSortableString(ti.Track.FileName)).ToList();
                        break;
                    case TrackOrder.ByRating:
                        orderedTracks = tracks.OrderByDescending(ti => ti.Track.Rating.HasValue ? ti.Track.Rating : 0).ToList();
                        break;
                    case TrackOrder.ReverseAlphabetical:
                        orderedTracks = tracks.OrderByDescending(ti => !string.IsNullOrEmpty(GetSortableString(ti.Track.TrackTitle)) ? GetSortableString(ti.Track.TrackTitle) : GetSortableString(ti.Track.FileName)).ToList();
                        break;
                    case TrackOrder.None:
                        orderedTracks = tracks.ToList();
                        break;
                    default:
                        // By album
                        orderedTracks = tracks.OrderBy(ti => GetSortableString(ti.Album.AlbumTitle)).ThenBy(ti => ti.Track.DiscNumber > 0 ? ti.Track.DiscNumber : 1).ThenBy(ti => ti.Track.TrackNumber).ToList();
                        break;
                }
            });

            return orderedTracks;
        }
    }
}
