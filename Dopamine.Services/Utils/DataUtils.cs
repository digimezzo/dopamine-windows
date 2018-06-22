using Dopamine.Core.Utils;
using Dopamine.Data;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Utils
{
    public static class DataUtils
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

        public static bool FilterTracks(PlayableTrack track, string filter)
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

        public static async Task<List<Album>> OrderAlbumsAsync(IList<Album> albums, AlbumOrder albumOrder)
        {
            var orderedAlbums = new List<Album>();

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
                        orderedAlbums = albums.OrderByDescending((a) => a.DateCreated).ToList();
                        break;
                    case AlbumOrder.ByAlbumArtist:
                        orderedAlbums = albums.OrderBy((a) => FormatUtils.GetSortableString(a.AlbumArtist, true)).ToList();
                        break;
                    case AlbumOrder.ByYear:
                        orderedAlbums = albums.OrderByDescending((a) => a.Year != null ? a.Year : 0).ToList();
                        break;
                    default:
                        // Alphabetical
                        orderedAlbums = albums.OrderBy((a) => FormatUtils.GetSortableString(a.AlbumTitle)).ToList();
                        break;
                }
            });

            return orderedAlbums;
        }

        public static async Task<List<PlayableTrack>> OrderTracksAsync(IList<PlayableTrack> tracks, TrackOrder trackOrder)
        {
            var orderedTracks = new List<PlayableTrack>();

            await Task.Run(() =>
            {
                switch (trackOrder)
                {
                    case TrackOrder.Alphabetical:
                        orderedTracks = tracks.OrderBy((t) => !string.IsNullOrEmpty(FormatUtils.GetSortableString(t.TrackTitle)) ? FormatUtils.GetSortableString(t.TrackTitle) : FormatUtils.GetSortableString(t.FileName)).ToList();
                        break;
                    case TrackOrder.ByAlbum:
                        orderedTracks = tracks.OrderBy((t) => FormatUtils.GetSortableString(t.AlbumArtist)).ThenBy((t) => FormatUtils.GetSortableString(t.AlbumTitle)).ThenBy((t) => t.DiscNumber > 0 ? t.DiscNumber : 1).ThenBy((t) => t.TrackNumber).ToList();
                        break;
                    case TrackOrder.ByFileName:
                        orderedTracks = tracks.OrderBy((t) => FormatUtils.GetSortableString(t.FileName)).ToList();
                        break;
                    case TrackOrder.ByRating:
                        orderedTracks = tracks.OrderByDescending((t) => t.Rating.HasValue ? t.Rating : 0).ToList();
                        break;
                    case TrackOrder.ReverseAlphabetical:
                        orderedTracks = tracks.OrderByDescending((t) => !string.IsNullOrEmpty(FormatUtils.GetSortableString(t.TrackTitle)) ? FormatUtils.GetSortableString(t.TrackTitle) : FormatUtils.GetSortableString(t.FileName)).ToList();
                        break;
                    case TrackOrder.None:
                        orderedTracks = tracks.ToList();
                        break;
                    default:
                        // By album
                        orderedTracks = tracks.OrderBy((t) => FormatUtils.GetSortableString(t.AlbumTitle)).ThenBy((t) => t.DiscNumber > 0 ? t.DiscNumber : 1).ThenBy((t) => t.TrackNumber).ToList();
                        break;
                }
            });

            return orderedTracks;
        }
    }
}
