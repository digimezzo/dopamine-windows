using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Data.Contracts.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Core.Utils
{
    public class MetadataUtils
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


        public static string SanitizeTag(string str)
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

        public static long SafeConvertToLong(string str)
        {

            long result = 0;
            Int64.TryParse(str, out result);
            return result;
        }

        public static byte Star2PopMRating(int rating)
        {

            // 5 stars = POPM 255
            // 4 stars = POPM 196
            // 3 stars = POPM 128
            // 2 stars = POPM 64
            // 1 stars = POPM 1
            // 0 stars = POPM 0

            switch (rating)
            {
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 64;
                case 3:
                    return 128;
                case 4:
                    return 196;
                case 5:
                    return 255;
                default:
                    // Should not happen
                    return 0;
            }
        }

        public static int PopM2StarRating(byte popMRating)
        {

            // 0 stars = POPM 0
            // 1 stars = POPM 1
            // 2 stars = POPM 64
            // 3 stars = POPM 128
            // 4 stars = POPM 196
            // 5 stars = POPM 255

            if (popMRating <= 0)
            {
                return 0;
            }
            else if (popMRating <= 1)
            {
                return 1;
            }
            else if (popMRating <= 64)
            {
                return 2;
            }
            else if (popMRating <= 128)
            {
                return 3;
            }
            else if (popMRating <= 196)
            {
                return 4;
            }
            else if (popMRating <= 255)
            {
                return 5;
            }
            else
            {
                return 0;
                // Should not happen
            }
        }

        public static void SplitMetadata(string path, ref Track track, ref TrackStatistic trackStatistic, ref Album album, ref Artist artist, ref Genre genre)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var fmd = new FileMetadata(path);

                // Track information
                track.Path = path;
                track.SafePath = path.ToSafePath();
                track.FileName = FileUtils.NameWithoutExtension(path);
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
                track.HasLyrics = string.IsNullOrWhiteSpace(fmd.Lyrics.Value) ? 0 : 1;
                track.NeedsIndexing = 0;

                // TrackStatistic information
                trackStatistic.Path = path;
                trackStatistic.SafePath = path.ToSafePath();
                trackStatistic.Rating = fmd.Rating.Value;

                // Before proceeding, get the available artists
                string albumArtist = GetFirstAlbumArtist(fmd);
                string trackArtist = GetFirstArtist(fmd); // will be used for the album if no album artist is found

                // Album information
                album.AlbumTitle = string.IsNullOrWhiteSpace(fmd.Album.Value) ? Defaults.UnknownAlbumText : MetadataUtils.SanitizeTag(fmd.Album.Value);
                album.AlbumArtist = (albumArtist == Defaults.UnknownArtistText ? trackArtist : albumArtist);
                album.DateAdded = DateTime.Now.Ticks;
                album.DateCreated = FileUtils.DateCreatedTicks(path);

                UpdateAlbumYear(album, MetadataUtils.SafeConvertToLong(fmd.Year.Value));

                // Artist information
                artist.ArtistName = trackArtist;

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
                track.FileSize = FileUtils.SizeInBytes(path);
                track.DateFileModified = FileUtils.DateModifiedTicks(path);
                track.DateLastSynced = DateTime.Now.Ticks;
            }
        }

        public static async Task<PlayableTrack> Path2TrackAsync(string path, TrackStatistic savedTrackStatistic)
        {
            var returnTrack = new PlayableTrack();

            await Task.Run(() =>
            {
                var track = new Track();
                var trackStatistic = new TrackStatistic();
                var album = new Album();
                var artist = new Artist();
                var genre = new Genre();

                MetadataUtils.SplitMetadata(path, ref track, ref trackStatistic, ref album, ref artist, ref genre);

                returnTrack.Path = track.Path;
                returnTrack.SafePath = track.Path.ToSafePath();
                returnTrack.FileName = track.FileName;
                returnTrack.MimeType = track.MimeType;
                returnTrack.FileSize = track.FileSize;
                returnTrack.BitRate = track.BitRate;
                returnTrack.SampleRate = track.SampleRate;
                returnTrack.TrackTitle = track.TrackTitle;
                returnTrack.TrackNumber = track.TrackNumber;
                returnTrack.TrackCount = track.TrackCount;
                returnTrack.DiscNumber = track.DiscNumber;
                returnTrack.DiscCount = track.DiscCount;
                returnTrack.Duration = track.Duration;
                returnTrack.Year = track.Year;
                returnTrack.HasLyrics = track.HasLyrics;

                returnTrack.ArtistName = artist.ArtistName;

                returnTrack.GenreName = genre.GenreName;

                returnTrack.AlbumTitle = album.AlbumTitle;
                returnTrack.AlbumArtist = album.AlbumArtist;
                returnTrack.AlbumYear = album.Year;

                // If there is a saved TrackStatistic, used that one. Otherwise the
                // TrackStatistic from the file is used. That only contains rating.
                if (savedTrackStatistic != null) trackStatistic = savedTrackStatistic;

                returnTrack.Rating = trackStatistic.Rating;
                returnTrack.Love = trackStatistic.Love;
                returnTrack.PlayCount = trackStatistic.PlayCount;
                returnTrack.SkipCount = trackStatistic.SkipCount;
                returnTrack.DateLastPlayed = trackStatistic.DateLastPlayed;
            });

            return returnTrack;
        }

        public static string GetFirstGenre(FileMetadata fmd)
        {
            return string.IsNullOrWhiteSpace(fmd.Genres.Value) ? Defaults.UnknownGenreText : MetadataUtils.PatchID3v23Enumeration(fmd.Genres.Values).FirstNonEmpty(Defaults.UnknownGenreText);
        }

        public static string GetFirstArtist(FileMetadata iFileMetadata)
        {
            return string.IsNullOrWhiteSpace(iFileMetadata.Artists.Value) ? Defaults.UnknownArtistText : MetadataUtils.SanitizeTag(MetadataUtils.PatchID3v23Enumeration(iFileMetadata.Artists.Values).FirstNonEmpty(Defaults.UnknownArtistText));
        }

        public static string GetFirstAlbumArtist(FileMetadata iFileMetadata)
        {
            return string.IsNullOrWhiteSpace(iFileMetadata.AlbumArtists.Value) ? Defaults.UnknownArtistText : MetadataUtils.SanitizeTag(MetadataUtils.PatchID3v23Enumeration(iFileMetadata.AlbumArtists.Values).FirstNonEmpty(Defaults.UnknownArtistText));
        }

        public static bool UpdateAlbumYear(Album album, long year)
        {
            if (!album.AlbumTitle.Equals(Defaults.UnknownAlbumText) && year > 0 && (album.Year == null || album.Year != year))
            {
                album.Year = year;
                return true;
            }

            return false;
        }
    }
}
