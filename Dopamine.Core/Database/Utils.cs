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

        public static bool FilterTracks(MergedTrack track, string filter)
        {
            // Trim is required here, otherwise the filter might flip on the space at the beginning (and probably at the end)
            string[] pieces = filter.Trim().Split(Convert.ToChar(" "));

            // Just making sure that all fields are not Nothing
            if (track.TrackTitle == null) track.TrackTitle = string.Empty;
            if (track.ArtistName == null) track.ArtistName = string.Empty;
            if (track.AlbumTitle == null) track.AlbumTitle = string.Empty;
            if (track.FileName == null) track.FileName = string.Empty;
            if (track.Year == null) track.Year = 0;

            return pieces.All((s) => track.TrackTitle.ToLower().Contains(s.ToLower()) | track.ArtistName.ToLower().Contains(s.ToLower()) | track.AlbumTitle.ToLower().Contains(s.ToLower()) | track.FileName.ToLower().Contains(s.ToLower()) | track.Year.ToString().Contains(s.ToLower()));
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

        public static async Task<List<Artist>> OrderArtistsAsync(IList<Artist> artists, ArtistOrder artistOrder)
        {
            var orderedArtists = new List<Artist>();

            await Task.Run(() =>
            {
                switch (artistOrder)
                {
                    case ArtistOrder.Alphabetical:
                        orderedArtists = artists.OrderBy((a) => Utils.GetSortableString(a.ArtistName, true)).ToList();
                        break;
                    case ArtistOrder.ReverseAlphabetical:
                        orderedArtists = artists.OrderByDescending((a) => Utils.GetSortableString(a.ArtistName, true)).ToList();
                        break;
                    default:
                        // Alphabetical
                        orderedArtists = artists.OrderBy((a) => Utils.GetSortableString(a.ArtistName, true)).ToList();
                        break;
                }
            });

            return orderedArtists;
        }

        public static async Task<List<Genre>> OrderGenresAsync(IList<Genre> genres, GenreOrder genreOrder)
        {
            var orderedGenres = new List<Genre>();

            await Task.Run(() =>
            {
                switch (genreOrder)
                {
                    case GenreOrder.Alphabetical:
                        orderedGenres = genres.OrderBy((g) => Utils.GetSortableString(g.GenreName, true)).ToList();
                        break;
                    case GenreOrder.ReverseAlphabetical:
                        orderedGenres = genres.OrderByDescending((g) => Utils.GetSortableString(g.GenreName, true)).ToList();
                        break;
                    default:
                        // Alphabetical
                        orderedGenres = genres.OrderBy((g) => Utils.GetSortableString(g.GenreName, true)).ToList();
                        break;
                }
            });

            return orderedGenres;
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

        public static async Task<List<MergedTrack>> OrderTracksAsync(IList<MergedTrack> tracks, TrackOrder trackOrder)
        {
            var orderedTracks = new List<MergedTrack>();

            await Task.Run(() =>
            {
                switch (trackOrder)
                {
                    case TrackOrder.Alphabetical:
                        orderedTracks = tracks.OrderBy((t) => !string.IsNullOrEmpty(GetSortableString(t.TrackTitle)) ? GetSortableString(t.TrackTitle) : GetSortableString(t.FileName)).ToList();
                        break;
                    case TrackOrder.ByAlbum:
                        orderedTracks = tracks.OrderBy((t) => GetSortableString(t.AlbumArtist)).ThenBy((t) => GetSortableString(t.AlbumTitle)).ThenBy((t) => t.DiscNumber > 0 ? t.DiscNumber : 1).ThenBy((t) => t.TrackNumber).ToList();
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
            var str = string.Join(",", list.Select((item) => "'" + item.Replace("'","''") + "'").ToArray());
            return str;
        }
    }
}
