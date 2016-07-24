using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Extensions;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
using System;
using System.IO;

namespace Dopamine.Common.Services.Indexing
{
    public static class IndexerUtils
    {
        public static bool IsTrackOutdated(Track track)
        {
            if (track.FileSize == null || track.FileSize != FileOperations.GetFileSize(track.Path) || track.DateFileModified < FileOperations.GetDateModified(track.Path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetExternalArtworkPath(string path)
        {
            string directory = Path.GetDirectoryName(path);

            foreach (string externalCoverArtPattern in Defaults.ExternalCoverArtPatterns)
            {
                if (System.IO.File.Exists(Path.Combine(directory, externalCoverArtPattern)))
                {
                    return Path.Combine(directory, externalCoverArtPattern);
                }
            }

            return string.Empty;
        }

        public static byte[] GetEmbeddedArtwork(string path)
        {

            byte[] artworkData = null;

            try
            {
                var fmd = new FileMetadata(path);
                artworkData = fmd.ArtworkData.DataValue;
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem while getting artwork data for Track with path='{0}'. Exception: {1}", path, ex.Message);
            }

            return artworkData;
        }

        public static byte[] GetExternalArtwork(string path)
        {
            byte[] artworkData = null;

            try
            {
                string externalArtworkPath = GetExternalArtworkPath(path);

                if (!string.IsNullOrEmpty(externalArtworkPath))
                {
                    artworkData = ImageOperations.Image2ByteArray(externalArtworkPath);
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem while getting external artwork for Track with path='{0}'. Exception: {1}", path, ex.Message);
            }

            return artworkData;
        }

        public static string CacheArtworkData(byte[] artworkData)
        {
            string coverArtCacheSubDirectory = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.CacheSubDirectory, ApplicationPaths.CoverArtCacheSubDirectory);
            string artworkID = "album-" + Guid.NewGuid().ToString();

            ImageOperations.Byte2Jpg(artworkData, Path.Combine(coverArtCacheSubDirectory, artworkID + ".jpg"), 0, 0, Constants.CoverQualityPercent);

            return artworkID;
        }

        public static bool CacheArtwork(Album album, string path)
        {
            bool isCached = false;

            try
            {
                // Don't set album art is the album is unknown
                if (!album.AlbumTitle.Equals(Defaults.UnknownAlbumString))
                {
                    // Get embedded artwork
                    byte[] artworkData = GetEmbeddedArtwork(path);


                    if (artworkData == null || artworkData.Length == 0)
                    {
                        // If getting embedded artwork failed, try to get external artwork
                        artworkData = GetExternalArtwork(path);
                    }

                    if (artworkData != null)
                    {
                        album.ArtworkID = CacheArtworkData(artworkData);
                        album.DateLastSynced = DateTime.Now.Ticks;
                        isCached = true;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(album.ArtworkID))
                    {
                        album.ArtworkID = string.Empty;
                        album.DateLastSynced = DateTime.Now.Ticks;
                    }
                }

            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not cache artwork for Album with Title='{0}' and Album artist='{1}'. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
            }

            return isCached;
        }

        public static long CalculateSaveItemCount(long numberItemsToAdd)
        {
            if (numberItemsToAdd < 50000)
            {
                return 5000;
                // Every 5000 items
            }
            else
            {
                return Convert.ToInt64((10 / 100) * numberItemsToAdd);
                // Save every 10 %
            }
        }

        public static void SplitMetadata(string path, ref Track track, ref Album album, ref Artist artist, ref Genre genre)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var fmd = new FileMetadata(path);
                var fi = new FileInformation(path);

                // Track information
                track.Path = path;
                track.FileName = fi.NameWithoutExtension;
                track.Duration = Convert.ToInt64(fmd.Duration.TotalMilliseconds);
                track.MimeType = fmd.MimeType;
                track.BitRate = fmd.BitRate;
                track.SampleRate = fmd.SampleRate;
                track.TrackTitle = MetadataUtils.SanitizeTag(fmd.Title.Value);
                track.TrackNumber = MetadataUtils.SafeConvertToLong(fmd.TrackNumber.Value);
                track.TrackCount = MetadataUtils.SafeConvertToLong(fmd.TrackCount.Value);
                track.DiscNumber = MetadataUtils.SafeConvertToLong(fmd.DiscNumber.Value);
                track.DiscCount = MetadataUtils.SafeConvertToLong(fmd.DiscCount.Value);
                track.Year = MetadataUtils.SafeConvertToLong(fmd.Year.Value);
                track.Rating = fmd.Rating.Value;

                // Album information
                album.AlbumTitle = string.IsNullOrEmpty(fmd.Album.Value) ? Defaults.UnknownAlbumString : MetadataUtils.SanitizeTag(fmd.Album.Value);
                album.AlbumArtist = GetFirstAlbumArtist(fmd);
                album.DateAdded = FileOperations.GetDateCreated(path);

                IndexerUtils.UpdateAlbumYear(album, MetadataUtils.SafeConvertToLong(fmd.Year.Value));

                // Artist information
                artist.ArtistName = GetFirstArtist(fmd);

                // Genre information
                genre.GenreName = GetFirstGenre(fmd);

                // Metadata hash
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append(album.AlbumTitle);
                sb.Append(artist.ArtistName);
                sb.Append(genre.GenreName);
                sb.Append(track.TrackTitle);
                sb.Append(track.TrackNumber);
                sb.Append(track.Year);
                track.MetaDataHash = CryptographyUtils.MD5Hash(sb.ToString());

                // File information
                track.FileSize = fi.SizeInBytes;
                track.DateFileModified = fi.DateModifiedTicks;
                track.DateLastSynced = DateTime.Now.Ticks;
            }
        }

        public static string GetFirstGenre(FileMetadata fmd)
        {
            return string.IsNullOrEmpty(fmd.Genres.Value) ? Defaults.UnknownGenreString : MetadataUtils.PatchID3v23Enumeration(fmd.Genres.Values).FirstNonEmpty(Defaults.UnknownGenreString);
        }

        public static string GetFirstArtist(FileMetadata iFileMetadata)
        {

            return string.IsNullOrEmpty(iFileMetadata.Artists.Value) ? Defaults.UnknownArtistString : MetadataUtils.SanitizeTag(MetadataUtils.PatchID3v23Enumeration(iFileMetadata.Artists.Values).FirstNonEmpty(Defaults.UnknownArtistString));
        }

        public static string GetFirstAlbumArtist(FileMetadata iFileMetadata)
        {
            return string.IsNullOrEmpty(iFileMetadata.AlbumArtists.Value) ? Defaults.UnknownAlbumArtistString : MetadataUtils.SanitizeTag(MetadataUtils.PatchID3v23Enumeration(iFileMetadata.AlbumArtists.Values).FirstNonEmpty(Defaults.UnknownAlbumArtistString));
        }

        public static TrackInfo Path2TrackInfo(string path, string artworkPrefix)
        {
            var ti = new TrackInfo();

            try
            {
                var fmd = new FileMetadata(path);
                var fi = new FileInformation(path);

                ti.Track = new Track
                {
                    Path = path,
                    FileName = fi.NameWithoutExtension,
                    MimeType = fmd.MimeType,
                    FileSize = fi.SizeInBytes,
                    BitRate = fmd.BitRate,
                    SampleRate = fmd.SampleRate,
                    TrackTitle = MetadataUtils.SanitizeTag(fmd.Title.Value),
                    TrackNumber = MetadataUtils.SafeConvertToLong(fmd.TrackNumber.Value),
                    TrackCount = MetadataUtils.SafeConvertToLong(fmd.TrackCount.Value),
                    DiscNumber = MetadataUtils.SafeConvertToLong(fmd.DiscNumber.Value),
                    DiscCount = MetadataUtils.SafeConvertToLong(fmd.DiscCount.Value),
                    Duration = Convert.ToInt64(fmd.Duration.TotalMilliseconds),
                    Year = MetadataUtils.SafeConvertToLong(fmd.Year.Value)
                };

                ti.Artist = new Artist { ArtistName = GetFirstArtist(fmd) };

                ti.Genre = new Genre { GenreName = GetFirstGenre(fmd) };

                ti.Album = new Album
                {
                    AlbumTitle = string.IsNullOrEmpty(fmd.Album.Value) ? Defaults.UnknownAlbumString : MetadataUtils.SanitizeTag(fmd.Album.Value),
                    AlbumArtist = GetFirstAlbumArtist(fmd)
                };

                IndexerUtils.UpdateAlbumYear(ti.Album, MetadataUtils.SafeConvertToLong(fmd.Year.Value));

                IndexerUtils.CacheArtwork(ti.Album, ti.Track.Path);

            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Error while creating TrackInfo from file '{0}'. Exception: {1}", path, ex.Message);

                // Make sure the file can be opened by creating a TrackInfo with some default values
                ti = new TrackInfo();

                ti.Track = new Track
                {
                    Path = path,
                    FileName = System.IO.Path.GetFileNameWithoutExtension(path)
                };

                ti.Artist = new Artist { ArtistName = Defaults.UnknownArtistString };

                ti.Genre = new Genre { GenreName = Defaults.UnknownGenreString };

                ti.Album = new Album
                {
                    AlbumTitle = Defaults.UnknownAlbumString,
                    AlbumArtist = Defaults.UnknownAlbumArtistString
                };
            }

            return ti;
        }

        public static int CalculatePercent(long currentValue, long totalValue)
        {
            int percent = 0;

            if (totalValue > 0)
            {
                percent = Convert.ToInt32((currentValue / totalValue) * 100);
            }

            return percent;
        }

        public static bool UpdateAlbumYear(Album album, long year)
        {
            if (!album.AlbumTitle.Equals(Defaults.UnknownAlbumString) && year > 0 && (album.Year == null || album.Year != year))
            {
                album.Year = year;
                return true;
            }

            return false;
        }
    }

}
