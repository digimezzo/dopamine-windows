using Digimezzo.Utilities.Settings;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public sealed class Utils
    {
        public static void GetVisibleSongsColumns(ref bool ratingVisible, ref bool loveVisible, ref bool lyricsVisible, ref bool artistVisible, ref bool albumVisible, ref bool genreVisible, ref bool lengthVisible, ref bool playCountVisible, ref bool skipCountVisible, ref bool dateLastPlayedVisible, ref bool albumArtistVisible, ref bool trackNumberVisible, ref bool yearVisible, ref bool bitrateVisible)
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

        public static void SetVisibleSongsColumns(bool ratingVisible, bool loveVisible, bool lyricsVisible, bool artistVisible, bool albumVisible, bool genreVisible, bool lengthVisible, bool playCountVisible, bool skipCountVisible, bool dateLastPlayedVisible, bool albumArtistVisible, bool trackNumberVisible, bool yearVisible, bool bitrateVisible)
        {
            List<string> visibleColumns = new List<string>();

            if (ratingVisible)
                visibleColumns.Add("rating");
            if (loveVisible)
                visibleColumns.Add("love");
            if (lyricsVisible)
                visibleColumns.Add("lyrics");
            if (artistVisible)
                visibleColumns.Add("artist");
            if (albumVisible)
                visibleColumns.Add("album");
            if (genreVisible)
                visibleColumns.Add("genre");
            if (lengthVisible)
                visibleColumns.Add("length");
            if (playCountVisible)
                visibleColumns.Add("playcount");
            if (skipCountVisible)
                visibleColumns.Add("skipcount");
            if (dateLastPlayedVisible)
                visibleColumns.Add("datelastplayed");
            if (albumArtistVisible)
                visibleColumns.Add("albumartist");
            if (trackNumberVisible)
                visibleColumns.Add("tracknumber");
            if (yearVisible)
                visibleColumns.Add("year");
            if (bitrateVisible)
                visibleColumns.Add("bitrate");

            SettingsClient.Set<string>("TracksGrid", "VisibleColumns", string.Join(";", visibleColumns.ToArray()));
        }
    }
}
