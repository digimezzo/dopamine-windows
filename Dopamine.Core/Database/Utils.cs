using Dopamine.Core.Database.Entities;
using Dopamine.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Database
{
    public sealed class Utils
    {
        public static bool FilterAlbums(Album album, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (album.AlbumTitle == null) album.AlbumTitle = string.Empty;
            if (album.AlbumArtist == null) album.AlbumArtist = string.Empty;
            if (album.Year == null) album.Year = 0;

            return pieces.All((s) => album.AlbumTitle.ToLower().Contains(s.ToLower()) | album.AlbumArtist.ToLower().Contains(s.ToLower()) | album.Year.ToString().ToLower().Contains(s.ToLower()));
        }

        public static bool FilterArtists(Artist artist, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (artist.ArtistName == null) artist.ArtistName = string.Empty;

            return pieces.All((s) => artist.ArtistName.ToLower().Contains(s.ToLower()));
        }

        public static bool FilterGenres(Genre genre, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (genre.GenreName == null)
                genre.GenreName = string.Empty;

            return pieces.All((s) => genre.GenreName.ToLower().Contains(s.ToLower()));
        }

        public static bool FilterTracks(TrackInfo trackInfo, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (trackInfo.TrackTitle == null) trackInfo.TrackTitle = string.Empty;
            if (trackInfo.ArtistName == null) trackInfo.ArtistName = string.Empty;
            if (trackInfo.AlbumTitle == null) trackInfo.AlbumTitle = string.Empty;
            if (trackInfo.FileName == null) trackInfo.FileName = string.Empty;
            if (trackInfo.Year == null) trackInfo.Year = 0;

            return pieces.All((s) => trackInfo.TrackTitle.ToLower().Contains(s.ToLower()) | trackInfo.ArtistName.ToLower().Contains(s.ToLower()) | trackInfo.AlbumTitle.ToLower().Contains(s.ToLower()) | trackInfo.FileName.ToLower().Contains(s.ToLower()) | trackInfo.Year.ToString().Contains(s.ToLower()));
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
                        orderedAlbums = albums.OrderBy((a) => GetSortableString(a.AlbumTitle)).ToList();
                        break;
                    case AlbumOrder.ByDateAdded:
                        orderedAlbums = albums.OrderByDescending((a) => a.DateAdded).ToList();
                        break;
                    case AlbumOrder.ByAlbumArtist:
                        orderedAlbums = albums.OrderBy((a) => GetSortableString(a.AlbumArtist, true)).ToList();
                        break;
                    case AlbumOrder.ByYear:
                        orderedAlbums = albums.OrderByDescending((a) => a.Year != null ? a.Year : 0).ToList();
                        break;
                    default:
                        // Alphabetical
                        orderedAlbums = albums.OrderBy((a) => GetSortableString(a.AlbumTitle)).ToList();
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
                        orderedTracks = tracks.OrderBy((t) => !string.IsNullOrEmpty(GetSortableString(t.TrackTitle)) ? GetSortableString(t.TrackTitle) : GetSortableString(t.FileName)).ToList();
                        break;
                    case TrackOrder.ByAlbum:
                        orderedTracks = tracks.OrderBy((t) => GetSortableString(t.AlbumTitle)).ThenBy((t) => t.DiscNumber > 0 ? t.DiscNumber : 1).ThenBy((t) => t.TrackNumber).ToList();
                        break;
                    case TrackOrder.ByFileName:
                        orderedTracks = tracks.OrderBy((t) => GetSortableString(t.FileName)).ToList();
                        break;
                    case TrackOrder.ByRating:
                        orderedTracks = tracks.OrderByDescending((t) => t.Rating.HasValue ? t.Rating : 0).ToList();
                        break;
                    case TrackOrder.ReverseAlphabetical:
                        orderedTracks = tracks.OrderByDescending((t) => !string.IsNullOrEmpty(GetSortableString(t.TrackTitle)) ? GetSortableString(t.TrackTitle) : GetSortableString(t.FileName)).ToList();
                        break;
                    case TrackOrder.None:
                        orderedTracks = tracks.ToList();
                        break;
                    default:
                        // By album
                        orderedTracks = tracks.OrderBy((t) => GetSortableString(t.AlbumTitle)).ThenBy((t) => t.DiscNumber > 0 ? t.DiscNumber : 1).ThenBy((t) => t.TrackNumber).ToList();
                        break;
                }
            });

            return orderedTracks;
        }

        public static string ToQueryList(IList<long> list)
        {
            return string.Join(",", list.ToArray());
        }

        public static string ToQueryList(IList<string> list)
        {
            return string.Join(",", list.Select((item) => "'"+item+"'").ToArray());
        }
    }
}
