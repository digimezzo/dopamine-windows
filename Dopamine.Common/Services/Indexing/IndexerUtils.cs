using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database.Entities;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Metadata;
using System;
using System.IO;

namespace Dopamine.Common.Services.Indexing
{
    public static class IndexerUtils
    {
        public static bool IsTrackOutdated(Track track)
        {
            if (track.FileSize == null || track.FileSize != FileUtils.SizeInBytes(track.Path) || track.DateFileModified < FileUtils.DateModifiedTicks(track.Path))
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
                var filename = (Path.Combine(directory, externalCoverArtPattern.Replace("%filename%", Path.GetFileNameWithoutExtension(path))));

                if (System.IO.File.Exists(filename))
                {
                    return filename;
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
                artworkData = fmd.ArtworkData.Value;
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem while getting artwork data for Track with path='{0}'. Exception: {1}", path, ex.Message);
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
                    artworkData = ImageUtils.Image2ByteArray(externalArtworkPath);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem while getting external artwork for Track with path='{0}'. Exception: {1}", path, ex.Message);
            }

            return artworkData;
        }

        public static byte[] GetArtwork(Album album, string path)
        {
            byte[] artworkData = null;

            try
            {
                // Don't get artwork is the album is unknown
                if (!album.AlbumTitle.Equals(Defaults.UnknownAlbumString))
                {
                    // Get embedded artwork
                    artworkData = GetEmbeddedArtwork(path);

                    if (artworkData == null || artworkData.Length == 0)
                    {
                        // If getting embedded artwork failed, try to get external artwork.
                        artworkData = GetExternalArtwork(path);
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not get artwork for Album with Title='{0}' and Album artist='{1}'. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
            }

            return artworkData;
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

        public static int CalculatePercent(long currentValue, long totalValue)
        {
            int percent = 0;

            if (totalValue > 0)
            {
                percent = Convert.ToInt32((currentValue / totalValue) * 100);
            }

            return percent;
        }
    }

}
