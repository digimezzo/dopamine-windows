using Digimezzo.Foundation.Core.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public sealed class CollectionUtils
    {
        public static void GetVisibleSongsColumns(ref bool ratingVisible, ref bool loveVisible, ref bool lyricsVisible, ref bool artistVisible, ref bool albumVisible, ref bool genreVisible, ref bool lengthVisible, ref bool playCountVisible, ref bool skipCountVisible, ref bool dateLastPlayedVisible, ref bool dateAddedVisible, ref bool dateCreatedVisible, ref bool albumArtistVisible, ref bool trackNumberVisible, ref bool yearVisible, ref bool bitrateVisible)
        {
            ratingVisible = false;
            loveVisible = false;
            lyricsVisible = false;
            artistVisible = false;
            albumVisible = false;
            genreVisible = false;
            lengthVisible = false;
            playCountVisible = false;
            skipCountVisible = false;
            dateLastPlayedVisible = false;
            dateAddedVisible = false;
            dateCreatedVisible = false;
            albumArtistVisible = false;
            trackNumberVisible = false;
            yearVisible = false;
            bitrateVisible = false;

            string visibleColumnsSettings = SettingsClient.Get<string>("TracksGrid", "VisibleColumns");
            string[] visibleColumns = null;

            if (!string.IsNullOrEmpty(visibleColumnsSettings))
            {
                if (visibleColumnsSettings.Contains(";"))
                {
                    visibleColumns = visibleColumnsSettings.Split(';');
                }
                else
                {
                    visibleColumns = new string[] { visibleColumnsSettings };
                }
            }

            if (visibleColumns != null && visibleColumns.Count() > 0)
            {

                foreach (string visibleColumn in visibleColumns)
                {
                    switch (visibleColumn)
                    {
                        case "rating":
                            ratingVisible = true;
                            break;
                        case "love":
                            loveVisible = true;
                            break;
                        case "lyrics":
                            lyricsVisible = true;
                            break;
                        case "artist":
                            artistVisible = true;
                            break;
                        case "album":
                            albumVisible = true;
                            break;
                        case "genre":
                            genreVisible = true;
                            break;
                        case "length":
                            lengthVisible = true;
                            break;
                        case "playcount":
                            playCountVisible = true;
                            break;
                        case "skipcount":
                            skipCountVisible = true;
                            break;
                        case "datelastplayed":
                            dateLastPlayedVisible = true;
                            break;
                        case "dateadded":
                            dateAddedVisible = true;
                            break;
                        case "datecreated":
                            dateCreatedVisible = true;
                            break;
                        case "albumartist":
                            albumArtistVisible = true;
                            break;
                        case "tracknumber":
                            trackNumberVisible = true;
                            break;
                        case "year":
                            yearVisible = true;
                            break;
                        case "bitrate":
                            bitrateVisible = true;
                            break;
                    }
                }
            }
        }

        public static void SetVisibleSongsColumns(bool ratingVisible, bool loveVisible, bool lyricsVisible, bool artistVisible, bool albumVisible, bool genreVisible, bool lengthVisible, bool playCountVisible, bool skipCountVisible, bool dateLastPlayedVisible, bool dateAddedVisible, bool dateCreatedVisible, bool albumArtistVisible, bool trackNumberVisible, bool yearVisible, bool bitrateVisible)
        {
            List<string> visibleColumns = new List<string>();

            if (ratingVisible)
            {
                visibleColumns.Add("rating");
            }

            if (loveVisible)
            {
                visibleColumns.Add("love");
            }

            if (lyricsVisible)
            {
                visibleColumns.Add("lyrics");
            }

            if (artistVisible)
            {
                visibleColumns.Add("artist");
            }

            if (albumVisible)
            {
                visibleColumns.Add("album");
            }

            if (genreVisible)
            {
                visibleColumns.Add("genre");
            }

            if (lengthVisible)
            {
                visibleColumns.Add("length");
            }

            if (playCountVisible)
            {
                visibleColumns.Add("playcount");
            }

            if (skipCountVisible)
            {
                visibleColumns.Add("skipcount");
            }

            if (dateLastPlayedVisible)
            {
                visibleColumns.Add("datelastplayed");
            }

            if (dateAddedVisible)
            {
                visibleColumns.Add("dateadded");
            }

            if (dateCreatedVisible)
            {
                visibleColumns.Add("datecreated");
            }

            if (albumArtistVisible)
            {
                visibleColumns.Add("albumartist");
            }

            if (trackNumberVisible)
            {
                visibleColumns.Add("tracknumber");
            }

            if (yearVisible)
            {
                visibleColumns.Add("year");
            }

            if (bitrateVisible)
            {
                visibleColumns.Add("bitrate");
            }

            SettingsClient.Set<string>("TracksGrid", "VisibleColumns", string.Join(";", visibleColumns.ToArray()));
        }

        public static void GetColumnSorting(out string sortMemberPath, out ListSortDirection sortDirection)
        {
            string settingsSortColumn = SettingsClient.Get<string>("TracksGrid", "SortColumn");

            switch (settingsSortColumn)
            {
                case "song":
                    sortMemberPath = "TrackTitle";
                    break;
                case "rating":
                    sortMemberPath = "Rating";
                    break;
                case "love":
                    sortMemberPath = "Love";
                    break;
                case "lyrics":
                    sortMemberPath = "HasLyrics";
                    break;
                case "artist":
                    sortMemberPath = "SortArtistName";
                    break;
                case "album":
                    sortMemberPath = "SortAlbumTitle";
                    break;
                case "genre":
                    sortMemberPath = "Genre";
                    break;
                case "length":
                    sortMemberPath = "SortDuration";
                    break;
                case "playcount":
                    sortMemberPath = "SortPlayCount";
                    break;
                case "skipcount":
                    sortMemberPath = "SortSkipCount";
                    break;
                case "datelastplayed":
                    sortMemberPath = "SortDateLastPlayed";
                    break;
                case "dateadded":
                    sortMemberPath = "SortDateAdded";
                    break;
                case "datecreated":
                    sortMemberPath = "SortDateFileCreated";
                    break;
                case "albumartist":
                    sortMemberPath = "SortAlbumArtist";
                    break;
                case "tracknumber":
                    sortMemberPath = "SortTrackNumber";
                    break;
                case "year":
                    sortMemberPath = "Year";
                    break;
                case "bitrate":
                    sortMemberPath = "SortBitrate";
                    break;
                default:
                    sortMemberPath = string.Empty;
                    break;
            }

            // Sort direction
            string settingsSortDirection = SettingsClient.Get<string>("TracksGrid", "SortDirection");

            if (settingsSortDirection.Equals("descending"))
            {
                sortDirection = ListSortDirection.Descending;
            }
            else
            {
                sortDirection = ListSortDirection.Ascending;
            }
        }

        public static void SetColumnSorting(string sortMemberPath, ListSortDirection? sortDirection)
        {
            // Sort column
            string sortColumn = string.Empty;

            switch (sortMemberPath)
            {
                case "TrackTitle":
                    sortColumn = "song";
                    break;
                case "Rating":
                    sortColumn = "rating";
                    break;
                case "Love":
                    sortColumn = "love";
                    break;
                case "HasLyrics":
                    sortColumn = "lyrics";
                    break;
                case "SortArtistName":
                    sortColumn = "artist";
                    break;
                case "SortAlbumTitle":
                    sortColumn = "album";
                    break;
                case "Genre":
                    sortColumn = "genre";
                    break;
                case "SortDuration":
                    sortColumn = "length";
                    break;
                case "SortPlayCount":
                    sortColumn = "playcount";
                    break;
                case "SortSkipCount":
                    sortColumn = "skipcount";
                    break;
                case "SortDateLastPlayed":
                    sortColumn = "datelastplayed";
                    break;
                case "SortDateAdded":
                    sortColumn = "dateadded";
                    break;
                case "SortDateFileCreated":
                    sortColumn = "datecreated";
                    break;
                case "SortAlbumArtist":
                    sortColumn = "albumartist";
                    break;
                case "SortTrackNumber":
                    sortColumn = "tracknumber";
                    break;
                case "Year":
                    sortColumn = "year";
                    break;
                case "SortBitrate":
                    sortColumn = "bitrate";
                    break;
                default:
                    sortMemberPath = string.Empty;
                    break;
            }

            SettingsClient.Set<string>("TracksGrid", "SortColumn", sortColumn);

            // Sort direction
            string settingsSortDirection = "ascending";

            if (sortDirection.HasValue && sortDirection.Equals(ListSortDirection.Descending))
            {
                settingsSortDirection = "descending";
            }

            if (string.IsNullOrEmpty(sortMemberPath))
            {
                settingsSortDirection = string.Empty;
            }

            SettingsClient.Set<string>("TracksGrid", "SortDirection", settingsSortDirection);
        }
    }
}
