using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Helpers;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        #region Variables
        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public PlaylistRepository()
        {
            this.factory = new SQLiteConnectionFactory();
        }
        #endregion

        #region IPlaylistRepository
        public async Task<List<Playlist>> GetPlaylistsAsync()
        {
            var selectedPlaylists = new List<Playlist>();

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            selectedPlaylists = conn.Table<Playlist>().Select((p) => p).ToList();
                            selectedPlaylists = selectedPlaylists.OrderBy((p) => Utils.GetSortableString(p.PlaylistName)).ToList();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not get the Playlists. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return selectedPlaylists;
        }

        public async Task<AddPlaylistResult> AddPlaylistAsync(string playlistName)
        {
            AddPlaylistResult result = AddPlaylistResult.Success;

            string trimmedPlaylistName = playlistName.Trim();

            if (string.IsNullOrEmpty(trimmedPlaylistName))
            {
                LogClient.Instance.Logger.Info("Could not add the Playlist because no playlist name was provided");
                return AddPlaylistResult.Blank;
            }

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            Playlist newPlaylist = new Playlist { PlaylistName = trimmedPlaylistName };
                            var existingPlaylistCount = conn.Query<Playlist>("SELECT * FROM Playlist WHERE TRIM(PlaylistName)=?", newPlaylist.PlaylistName).Count();

                            if (existingPlaylistCount == 0)
                            {
                                conn.Insert(newPlaylist);
                                LogClient.Instance.Logger.Info("Added the Playlist {0}", trimmedPlaylistName);
                            }
                            else
                            {
                                LogClient.Instance.Logger.Info("Didn't add the Playlist {0} because it is already in the database", trimmedPlaylistName);
                                result = AddPlaylistResult.Duplicate;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not add the Playlist {0}. Exception: {1}", trimmedPlaylistName, ex.Message);
                            result = AddPlaylistResult.Error;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<Playlist> GetPlaylistAsync(string playlistName)
        {
            Playlist dbPlaylist = null;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        dbPlaylist = conn.Query<Playlist>("SELECT * FROM Playlist WHERE TRIM(PlaylistName)=?", playlistName.Trim()).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return dbPlaylist;
        }

        public async Task<DeletePlaylistResult> DeletePlaylistsAsync(IList<Playlist> playlists)
        {
            DeletePlaylistResult result = DeletePlaylistResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        foreach (Playlist pl in playlists)
                        {
                            string trimmedPlaylistName = pl.PlaylistName.Trim();

                            // Get the Playlist in the database
                            Playlist dbPlaylist = conn.Query<Playlist>("SELECT * FROM Playlist WHERE TRIM(PlaylistName)=?", trimmedPlaylistName).FirstOrDefault();


                            if (dbPlaylist != null)
                            {
                                // Get the PlaylistEntries which contain the PlaylistID of the Playlist to delete
                                List<PlaylistEntry> playlistEntries = conn.Table<PlaylistEntry>().Where((p) => p.PlaylistID == dbPlaylist.PlaylistID).Select((p) => p).ToList();

                                conn.Delete(dbPlaylist);

                                foreach (var entry in playlistEntries)
                                {
                                    conn.Delete(entry);
                                }

                                LogClient.Instance.Logger.Info("Deleted the Playlist {0}", trimmedPlaylistName);
                            }
                            else
                            {
                                LogClient.Instance.Logger.Error("The playlist {0} could not be deleted because it was not found in the database.", trimmedPlaylistName);
                                result = DeletePlaylistResult.Error;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<RenamePlaylistResult> RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName)
        {
            RenamePlaylistResult result = RenamePlaylistResult.Success;

            string trimmedOldPlaylistName = oldPlaylistName.Trim();
            string trimmedNewPlaylistName = newPlaylistName.Trim();

            if (string.IsNullOrEmpty(trimmedNewPlaylistName))
            {
                LogClient.Instance.Logger.Info("Could not rename the Playlist {0} because no new playlist name was provided", trimmedOldPlaylistName);
                return RenamePlaylistResult.Blank;
            }

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        // Check if there is already a playlist with that name
                        var existingPlaylistCount = conn.Query<Playlist>("SELECT * FROM Playlist WHERE TRIM(PlaylistName)=?", trimmedNewPlaylistName).Count();

                        if (existingPlaylistCount == 0)
                        {
                            Playlist playlistToRename = conn.Query<Playlist>("SELECT * FROM Playlist WHERE TRIM(PlaylistName)=?", trimmedOldPlaylistName).ToList().FirstOrDefault();

                            if (playlistToRename != null)
                            {
                                playlistToRename.PlaylistName = trimmedNewPlaylistName;
                                conn.Update(playlistToRename);

                                result = RenamePlaylistResult.Success;
                            }
                            else
                            {
                                LogClient.Instance.Logger.Error("The playlist {0} could not be renamed because it was not found in the database.", trimmedOldPlaylistName);
                                result = RenamePlaylistResult.Error;
                            }
                        }
                        else
                        {
                            result = RenamePlaylistResult.Duplicate;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<AddToPlaylistResult> AddTracksToPlaylistAsync(IList<TrackInfo> tracks, string playlistName)
        {
            var result = new AddToPlaylistResult { IsSuccess = true };
            int numberTracksAdded = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            // Get the PlaylistID of the Playlist with PlaylistName = iPlaylistName
                            var playlistID = conn.Table<Playlist>().Select((p) => p).Where((p) => p.PlaylistName.Equals(playlistName)).ToList().Select((p) => p.PlaylistID).FirstOrDefault();

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks
                            foreach (TrackInfo ti in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry
                                {
                                    PlaylistID = playlistID,
                                    TrackID = ti.TrackID
                                };

                                if (!conn.Table<PlaylistEntry>().ToList().Contains(possiblePlaylistEntry))
                                {
                                    conn.Insert(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("A problem occured while adding Tracks to Playlist with name '{0}'. Exception: {1}", playlistName, ex.Message);
                            result.IsSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            result.NumberTracksAdded = numberTracksAdded;

            return result;
        }

        public async Task<AddToPlaylistResult> AddArtistsToPlaylistAsync(IList<Artist> artists, string playlistName)
        {
            var result = new AddToPlaylistResult { IsSuccess = true };
            int numberTracksAdded = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            // Get the PlaylistID of the Playlist with PlaylistName = iPlaylistName
                            var playlistID = conn.Table<Playlist>().Select((p) => p).Where((p) => p.PlaylistName.Equals(playlistName)).ToList().Select((p) => p.PlaylistID).FirstOrDefault();

                            // Get all the Tracks for the selected Artist
                            List<string> artistNames = artists.Select((a) => a.ArtistName).ToList();

                            string q = string.Format("SELECT DISTINCT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash FROM Track tra" +
                                                     " INNER JOIN Album alb ON tra.AlbumID=alb.AlbumID" +
                                                     " INNER JOIN Artist art ON tra.ArtistID=art.ArtistID" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE (alb.AlbumArtist IN({0}) OR art.ArtistName IN ({0})) AND fol.ShowInCollection=1;", Utils.ToQueryList(artistNames));

                            List<Track> tracks = conn.Query<Track>(q);

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks
                            foreach (Track trk in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry { PlaylistID = playlistID, TrackID = trk.TrackID };

                                if (!conn.Table<PlaylistEntry>().ToList().Contains(possiblePlaylistEntry))
                                {
                                    conn.Insert(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("A problem occured while adding Artists to Playlist with name '{0}'. Exception: {1}", playlistName, ex.Message);
                            result.IsSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            result.NumberTracksAdded = numberTracksAdded;

            return result;
        }

        public async Task<AddToPlaylistResult> AddGenresToPlaylistAsync(IList<Genre> genres, string playlistName)
        {
            var result = new AddToPlaylistResult { IsSuccess = true };
            int numberTracksAdded = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            // Get the PlaylistID of the Playlist with PlaylistName = iPlaylistName
                            var playlistID = conn.Table<Playlist>().Select((p) => p).Where((p) => p.PlaylistName.Equals(playlistName)).ToList().Select((p) => p.PlaylistID).FirstOrDefault();

                            // Get all the Tracks for the selected Genre
                            List<long> genreIDs = genres.Select((g) => g.GenreID).ToList();

                            string q = string.Format("SELECT DISTINCT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash FROM Track tra" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE tra.GenreID IN ({0}) AND fol.ShowInCollection=1", Utils.ToQueryList(genreIDs));

                            List<Track> tracks = conn.Query<Track>(q);

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks
                            foreach (Track trk in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry { PlaylistID = playlistID, TrackID = trk.TrackID };

                                if (!conn.Table<PlaylistEntry>().ToList().Contains(possiblePlaylistEntry))
                                {
                                    conn.Insert(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("A problem occured while adding Genres to Playlist with name '{0}'. Exception: {1}", playlistName, ex.Message);
                            result.IsSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            result.NumberTracksAdded = numberTracksAdded;

            return result;
        }

        public async Task<AddToPlaylistResult> AddAlbumsToPlaylistAsync(IList<Album> albums, string playlistName)
        {
            var result = new AddToPlaylistResult { IsSuccess = true };
            int numberTracksAdded = 0;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            // Get the Playlist with that PlaylistName
                            var playlistID = conn.Table<Playlist>().Select((p) => p).Where((p) => p.PlaylistName.Equals(playlistName)).ToList().Select((p) => p.PlaylistID).FirstOrDefault();

                            // Get all the Tracks for the selected Album
                            List<long> albumIDs = albums.Select((a) => a.AlbumID).ToList();

                            string q = string.Format("SELECT DISTINCT tra.TrackID, tra.ArtistID, tra.GenreID, tra.AlbumID, tra.FolderID, tra.Path," +
                                                     " tra.FileName, tra.MimeType, tra.FileSize, tra.BitRate, tra.SampleRate, tra.TrackTitle," +
                                                     " tra.TrackNumber, tra.TrackCount, tra.DiscNumber, tra.DiscCount, tra.Duration, tra.Year," +
                                                     " tra.Rating, tra.PlayCount, tra.SkipCount, tra.DateAdded, tra.DateLastPlayed, tra.DateLastSynced," +
                                                     " tra.DateFileModified, tra.MetaDataHash FROM Track tra" +
                                                     " INNER JOIN Folder fol ON tra.FolderID=fol.FolderID" +
                                                     " WHERE tra.AlbumID IN ({0}) AND fol.ShowInCollection=1", Utils.ToQueryList(albumIDs));

                            List<Track> tracks = conn.Query<Track>(q);

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks
                            foreach (Track trk in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry { PlaylistID = playlistID, TrackID = trk.TrackID };

                                if (!conn.Table<PlaylistEntry>().ToList().Contains(possiblePlaylistEntry))
                                {
                                    conn.Insert(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("A problem occured while adding Albums to Playlist with name '{0}'. Exception: {1}", playlistName, ex.Message);
                            result.IsSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            result.NumberTracksAdded = numberTracksAdded;

            return result;
        }

        public async Task<DeleteTracksFromPlaylistsResult> DeleteTracksFromPlaylistAsync(IList<TrackInfo> tracks, Playlist selectedPlaylist)
        {
            DeleteTracksFromPlaylistsResult result = DeleteTracksFromPlaylistsResult.Success;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        if (tracks != null)
                        {
                            foreach (TrackInfo ti in tracks)
                            {
                                try
                                {
                                    PlaylistEntry playlistEntryToDelete = conn.Table<PlaylistEntry>().Select((p) => p).Where((p) => p.TrackID.Equals(ti.TrackID) & p.PlaylistID.Equals(selectedPlaylist.PlaylistID)).FirstOrDefault();
                                    conn.Delete(playlistEntryToDelete);
                                }
                                catch (Exception ex)
                                {
                                    LogClient.Instance.Logger.Error("An error occured while deleting PlaylistEntry for Track '{0}'. Exception: {1}", ti.Path, ex.Message);
                                    result = DeleteTracksFromPlaylistsResult.Error;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return result;
        }

        public async Task<string> GetUniquePlaylistNameAsync(string name)
        {
            string uniqueName = name;

            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        int number = 1;

                        while (conn.Table<Playlist>().Select((p) => p.PlaylistName).ToList().Contains(uniqueName))
                        {
                            number += 1;
                            uniqueName = name + " " + number;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });

            return uniqueName;
        }

        #endregion
    }
}
