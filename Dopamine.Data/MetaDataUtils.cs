using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Utils;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Data
{
    public static class MetadataUtils
    {
        public static IEnumerable<string> PatchID3v23Enumeration(IEnumerable<string> valuesEnumer)
        {
            return JoinUnsplittableValues(valuesEnumer, Defaults.UnsplittableTagValues, '/');
        }

        public static IEnumerable<string> JoinUnsplittableValues(IEnumerable<string> valuesEnumer, ICollection<string> unsplittableValues, char separator)
        {
            if (valuesEnumer == null)
            {
                return null;
            }
            else
            {
                List<string> values = new List<string>(valuesEnumer);

                if ((values.Count == 0))
                    return null;

                foreach (string unsplittableValue in unsplittableValues)
                {
                    JoinUnsplittableValue(ref values, unsplittableValue, separator);
                }

                return values;
            }
        }

        public static int IndexOf<S, T>(IList<S> list, IList<T> subList, IEqualityComparer<S> comparer)
        {
            for (int i = 0; i <= list.Count - subList.Count; i++)
            {
                bool allEqual = true;

                for (int j = 0; j <= subList.Count - 1; j++)
                {
                    if (!comparer.Equals(list[i + j], (S)(object)subList[j]))
                    {
                        allEqual = false;
                        break;
                    }
                }

                if (allEqual) return i;
            }

            return -1;
        }

        public static string Join(string separator, IEnumerable values)
        {

            if (values == null) return string.Empty;

            IEnumerator enumer = values.GetEnumerator();
            if (!enumer.MoveNext()) return string.Empty;

            var result = new StringBuilder();

            while (true)
            {
                result.Append(enumer.Current.ToString());
                if (enumer.MoveNext())
                {
                    result.Append(separator);
                }
                else
                {
                    return result.ToString();
                }
            }
        }

        public static void JoinUnsplittableValue(ref List<string> valueList, string unsplittableValue, char separator)
        {
            IList<string> parts = unsplittableValue.Split(separator);

            int index = IndexOf<string, string>(valueList, parts, StringComparer.InvariantCultureIgnoreCase);

            if (index == -1)
                return;

            string[] origParts = new string[parts.Count];

            for (int i = 0; i <= parts.Count - 1; i++)
            {
                origParts[i] = valueList[index];
                valueList.RemoveAt(index);
            }

            valueList.Insert(index, string.Join(separator.ToString(), origParts));
        }

        public static long SafeConvertToLong(string str)
        {
            long result = 0;
            Int64.TryParse(str, out result);

            return result;
        }

        private static string GetMultiValueTags(MetadataValue value)
        {
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                return string.Empty;
            }

            IEnumerable<string> patchedEnumeration = MetadataUtils.PatchID3v23Enumeration(value.Values);
            IEnumerable<string> delimitedTags = patchedEnumeration.Select(x => FormatUtils.DelimitValue(x));

            return string.Join(string.Empty, delimitedTags.ToArray());
        }

        private static string GetAlbumTitle(FileMetadata fileMetadata)
        {
            return string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? string.Empty : FormatUtils.TrimValue(fileMetadata.Album.Value);
        }

        public static void FillTrackBase(FileMetadata fileMetadata, ref Track track)
        {
            track.TrackTitle = FormatUtils.TrimValue(fileMetadata.Title.Value);
            track.Year = SafeConvertToLong(fileMetadata.Year.Value);
            track.TrackNumber = SafeConvertToLong(fileMetadata.TrackNumber.Value);
            track.TrackCount = SafeConvertToLong(fileMetadata.TrackCount.Value);
            track.DiscNumber = SafeConvertToLong(fileMetadata.DiscNumber.Value);
            track.DiscCount = SafeConvertToLong(fileMetadata.DiscCount.Value);
            track.HasLyrics = string.IsNullOrWhiteSpace(fileMetadata.Lyrics.Value) ? 0 : 1;
            track.Artists = GetMultiValueTags(fileMetadata.Artists);
            track.Genres = GetMultiValueTags(fileMetadata.Genres);
            track.AlbumTitle = GetAlbumTitle(fileMetadata);
            track.AlbumArtists = GetMultiValueTags(fileMetadata.AlbumArtists);
            track.AlbumKey = GenerateInitialAlbumKey(track.AlbumTitle, track.AlbumArtists);
        }

        public static void FillTrack(FileMetadata fileMetadata, ref Track track)
        {
            string path = fileMetadata.Path;
            long nowTicks = DateTime.Now.Ticks;

            track.Path = path;
            track.SafePath = path.ToSafePath();
            track.FileName = FileUtils.FileNameWithoutExtension(path);
            track.Duration = Convert.ToInt64(fileMetadata.Duration.TotalMilliseconds);
            track.MimeType = fileMetadata.MimeType;
            track.BitRate = fileMetadata.BitRate;
            track.SampleRate = fileMetadata.SampleRate;
            track.NeedsIndexing = 0;
            track.NeedsAlbumArtworkIndexing = 0;
            track.FileSize = FileUtils.SizeInBytes(path);
            track.DateFileCreated = FileUtils.DateCreatedTicks(path);
            track.DateFileModified = FileUtils.DateModifiedTicks(path);
            track.DateAdded = nowTicks;
            track.DateLastSynced = nowTicks;
            track.Rating = fileMetadata.Rating.Value;

            FillTrackBase(fileMetadata, ref track);
        }

        private static string GenerateInitialAlbumKey(string albumTitle, string albumArtists)
        {
            if (string.IsNullOrWhiteSpace(albumTitle))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(albumArtists))
            {
                return string.Join(string.Empty, FormatUtils.DelimitValue(albumTitle), albumArtists.ToLower());
            }

            return FormatUtils.DelimitValue(albumTitle);
        }

        public static async Task<Track> Path2TrackAsync(string path)
        {
            var track = Track.CreateDefault(path);

            if (File.Exists(path))
            {
                FileMetadata fileMetadata = new FileMetadata(path);

                await Task.Run(() =>
                {
                    MetadataUtils.FillTrack(fileMetadata, ref track);
                });
            }

            return track;
        }
    }
}
