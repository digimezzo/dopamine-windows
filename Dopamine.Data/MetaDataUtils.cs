using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Utils;
using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public static string TrimTag(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return str.Trim();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string DelimitTag(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return $"{Constants.TagDelimiter}{str.Trim()}{Constants.TagDelimiter}";
            }
            else
            {
                return string.Empty;
            }
        }

        public static long SafeConvertToLong(string str)
        {
            long result = 0;
            Int64.TryParse(str, out result);

            return result;
        }

        private static string GetOrderedMultiValueTags(MetadataValue value)
        {
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                return string.Empty;
            }

            IEnumerable<string> patchedEnumeration = MetadataUtils.PatchID3v23Enumeration(value.Values);
            IEnumerable<string> delimitedTags = patchedEnumeration.Select(x => MetadataUtils.DelimitTag(x));

            return string.Join(string.Empty, delimitedTags.OrderBy(x => x).ToArray());
        }

        private static string GetAllArtists(IFileMetadata fileMetadata)
        {
            return GetOrderedMultiValueTags(fileMetadata.Artists);
        }

        private static string GetAllGenres(IFileMetadata fileMetadata)
        {
            return GetOrderedMultiValueTags(fileMetadata.Genres);
        }

        private static string GetAllAlbumArtists(IFileMetadata fileMetadata)
        {
            return GetOrderedMultiValueTags(fileMetadata.AlbumArtists);
        }

        public static void FillTrack(IFileMetadata fileMetadata, ref Track track)
        {
            string path = fileMetadata.Path;
            long nowTicks = DateTime.Now.Ticks;

            track.Path = path;
            track.SafePath = path.ToSafePath();
            track.FileName = FileUtils.NameWithoutExtension(path);
            track.Duration = Convert.ToInt64(fileMetadata.Duration.TotalMilliseconds);
            track.MimeType = fileMetadata.MimeType;
            track.BitRate = fileMetadata.BitRate;
            track.SampleRate = fileMetadata.SampleRate;
            track.TrackTitle = MetadataUtils.TrimTag(fileMetadata.Title.Value);
            track.TrackNumber = MetadataUtils.SafeConvertToLong(fileMetadata.TrackNumber.Value);
            track.TrackCount = MetadataUtils.SafeConvertToLong(fileMetadata.TrackCount.Value);
            track.DiscNumber = MetadataUtils.SafeConvertToLong(fileMetadata.DiscNumber.Value);
            track.DiscCount = MetadataUtils.SafeConvertToLong(fileMetadata.DiscCount.Value);
            track.Year = MetadataUtils.SafeConvertToLong(fileMetadata.Year.Value);
            track.HasLyrics = string.IsNullOrWhiteSpace(fileMetadata.Lyrics.Value) ? 0 : 1;
            track.NeedsIndexing = 0;
            track.NeedsAlbumArtworkIndexing = 0;
            track.FileSize = FileUtils.SizeInBytes(path);
            track.DateFileCreated = FileUtils.DateCreatedTicks(path);
            track.DateFileModified = FileUtils.DateModifiedTicks(path);
            track.DateAdded = nowTicks;
            track.DateLastSynced = nowTicks;
            track.Artists = GetAllArtists(fileMetadata);
            track.Genres = GetAllGenres(fileMetadata);
            track.AlbumTitle = string.IsNullOrWhiteSpace(fileMetadata.Album.Value) ? string.Empty : MetadataUtils.TrimTag(fileMetadata.Album.Value);
            track.AlbumArtists = GetAllAlbumArtists(fileMetadata);
            track.AlbumKey = GenerateInitialAlbumKey(track.AlbumTitle, track.AlbumArtists);
            track.Rating = fileMetadata.Rating.Value;
        }

        private static string GenerateInitialAlbumKey(string albumTitle, string albumArtists)
        {
            if (string.IsNullOrWhiteSpace(albumTitle))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(albumArtists))
            {
                return string.Join(string.Empty, DelimitTag(albumTitle), albumArtists);
            }

            return DelimitTag(albumTitle);
        }

        public static string GetCommaSeparatedMultiValueTags(string multiValueTagValue)
        {
            if (multiValueTagValue.Contains(Constants.DoubleTagDelimiter))
            {
                return string.Join(", ", GetMultiValueTagsCollection(multiValueTagValue));
            }

            return multiValueTagValue;
        }

        public static IEnumerable<string> GetMultiValueTagsCollection(string multiValueTagValue)
        {
            return multiValueTagValue.Split(Constants.DoubleTagDelimiter).Select(x => x.Trim(Constants.TagDelimiter));
        }

        public static async Task<Track> Path2TrackAsync(IFileMetadata fileMetadata)
        {
            var track = Track.CreateDefault(fileMetadata.Path);

            await Task.Run(() =>
            {
                MetadataUtils.FillTrack(fileMetadata, ref track);
            });

            return track;
        }
    }
}
