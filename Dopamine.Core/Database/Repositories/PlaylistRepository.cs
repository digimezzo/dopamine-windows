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
        #region IPlaylistRepository
        public async Task<List<Playlist>> GetPlaylistsAsync()
        {
            var selectedPlaylists = new List<Playlist>();

            await Task.Run(() =>
            {
                try
                {
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            selectedPlaylists = db.Playlists.Select((p) => p).ToList();
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            Playlist newPlaylist = new Playlist { PlaylistName = trimmedPlaylistName };

                            if (!db.Playlists.Select(pl => pl.PlaylistName.Trim()).Contains(newPlaylist.PlaylistName))
                            {
                                db.Playlists.Add(newPlaylist);
                                db.SaveChanges();
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        dbPlaylist = db.Playlists.Select((pl) => pl).Where((pl) => pl.PlaylistName.Trim().Equals(playlistName.Trim())).FirstOrDefault();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        foreach (Playlist pl in playlists)
                        {
                            string trimmedPlaylistName = pl.PlaylistName.Trim();

                            // Get the Playlist in the database
                            Playlist dbPlaylist = db.Playlists.Select((p) => p).Where((p) => p.PlaylistName.Trim().Equals(trimmedPlaylistName)).FirstOrDefault();


                            if (dbPlaylist != null)
                            {
                                // Get the PlaylistEntries which contain the PlaylistID of the Playlist to delete
                                List<PlaylistEntry> playlistEntries = db.PlaylistEntries.Where((p) => p.PlaylistID == dbPlaylist.PlaylistID).Select((p) => p).ToList();

                                db.Playlists.Remove(dbPlaylist);
                                db.PlaylistEntries.RemoveRange(playlistEntries);

                                LogClient.Instance.Logger.Info("Deleted the Playlist {0}", trimmedPlaylistName);
                            }
                            else
                            {
                                LogClient.Instance.Logger.Error("The playlist {0} could not be deleted because it was not found in the database.", trimmedPlaylistName);
                                result = DeletePlaylistResult.Error;
                            }
                        }

                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        // Check if there is already a playlist with that name
                        Playlist existingPlaylist = db.Playlists.Select((pl) => pl).Where((pl) => pl.PlaylistName.Trim().Equals(trimmedNewPlaylistName)).FirstOrDefault();

                        if (existingPlaylist == null)
                        {
                            Playlist playlistToRename = db.Playlists.Select((pl) => pl).Where((pl) => pl.PlaylistName.Trim().Equals(trimmedOldPlaylistName)).FirstOrDefault();

                            if (playlistToRename != null)
                            {
                                playlistToRename.PlaylistName = trimmedNewPlaylistName;
                                db.SaveChanges();

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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // Get the PlaylistID of the Playlist with PlaylistName = iPlaylistName
                            var playlistID = db.Playlists.Where((p) => p.PlaylistName.Equals(playlistName)).Select((p) => p.PlaylistID).FirstOrDefault();

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks
                            foreach (TrackInfo ti in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry
                                {
                                    PlaylistID = playlistID,
                                    TrackID = ti.Track.TrackID
                                };

                                if (!db.PlaylistEntries.ToList().Contains(possiblePlaylistEntry))
                                {
                                    db.PlaylistEntries.Add(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }

                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("Could not save the changes to the database. Exception: {0}", ex.Message);
                                result.IsSuccess = false;
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // Get the PlaylistID of the Playlist with PlaylistName = iPlaylistName
                            var playlistID = db.Playlists.Where((p) => p.PlaylistName.Equals(playlistName)).Select((p) => p.PlaylistID).FirstOrDefault();

                            // Get all the Tracks for the selected Artist
                            List<string> artistNames = artists.Select((a) => a.ArtistName).ToList();

                            List<Track> tracks = (from tra in db.Tracks
                                                  join alb in db.Albums on tra.AlbumID equals alb.AlbumID
                                                  join art in db.Artists on tra.ArtistID equals art.ArtistID
                                                  join fol in db.Folders on tra.FolderID equals fol.FolderID
                                                  where (artistNames.Contains(alb.AlbumArtist) | artistNames.Contains(art.ArtistName)) & fol.ShowInCollection == 1
                                                  select tra).ToList();

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks

                            foreach (Track trk in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry { PlaylistID = playlistID, TrackID = trk.TrackID };

                                if (!db.PlaylistEntries.ToList().Contains(possiblePlaylistEntry))
                                {
                                    db.PlaylistEntries.Add(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("Could not save the changes to the database. Exception: {0}", ex.Message);
                                result.IsSuccess = false;
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // Get the PlaylistID of the Playlist with PlaylistName = iPlaylistName
                            var playlistID = db.Playlists.Where((p) => p.PlaylistName.Equals(playlistName)).Select((p) => p.PlaylistID).FirstOrDefault();

                            // Get all the Tracks for the selected Genre
                            List<long> genreIds = genres.Select((g) => g.GenreID).ToList();

                            List<Track> tracks = (from tra in db.Tracks
                                                  join fol in db.Folders on tra.FolderID equals fol.FolderID
                                                  where genreIds.Contains(tra.GenreID) & fol.ShowInCollection == 1
                                                  select tra).ToList();

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks
                            foreach (Track trk in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry { PlaylistID = playlistID, TrackID = trk.TrackID };

                                if (!db.PlaylistEntries.ToList().Contains(possiblePlaylistEntry))
                                {
                                    db.PlaylistEntries.Add(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }

                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("Could not save the changes to the database. Exception: {0}", ex.Message);
                                result.IsSuccess = false;
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        try
                        {
                            // Get the PlaylistID of the Playlist with PlaylistName = iPlaylistName
                            var playlistID = db.Playlists.Where((p) => p.PlaylistName.Equals(playlistName)).Select((p) => p.PlaylistID).FirstOrDefault();

                            // Get all the Tracks for the selected Album
                            List<long> albumIds = albums.Select((a) => a.AlbumID).ToList();

                            List<Track> tracks = (from tra in db.Tracks
                                                  join fol in db.Folders on tra.FolderID equals fol.FolderID
                                                  where albumIds.Contains(tra.AlbumID) & fol.ShowInCollection == 1
                                                  select tra).ToList();

                            // Loop over the Tracks in iTracks and add an entry to PlaylistEntries for each of the Tracks
                            foreach (Track trk in tracks)
                            {
                                var possiblePlaylistEntry = new PlaylistEntry { PlaylistID = playlistID, TrackID = trk.TrackID };

                                if (!db.PlaylistEntries.ToList().Contains(possiblePlaylistEntry))
                                {
                                    db.PlaylistEntries.Add(possiblePlaylistEntry);
                                    numberTracksAdded += 1;
                                }
                            }

                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("Could not save the changes to the database. Exception: {0}", ex.Message);
                                result.IsSuccess = false;
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
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        if (tracks != null)
                        {
                            foreach (TrackInfo ti in tracks)
                            {
                                try
                                {
                                    PlaylistEntry playlistEntryToDelete = db.PlaylistEntries.Select((pe) => pe).Where((pe) => pe.TrackID.Equals(ti.Track.TrackID) & pe.PlaylistID.Equals(selectedPlaylist.PlaylistID)).FirstOrDefault();
                                    db.PlaylistEntries.Remove(playlistEntryToDelete);
                                }
                                catch (Exception ex)
                                {
                                    LogClient.Instance.Logger.Error("An error occured while deleting PlaylistEntry for Track '{0}'. Exception: {1}", ti.Track.Path, ex.Message);
                                    result = DeleteTracksFromPlaylistsResult.Error;
                                }
                            }
                        }

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("Could not save the changes to the database. Exception: {0}", ex.Message);
                            result = DeleteTracksFromPlaylistsResult.Error;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
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
                    using (var db = new DopamineContext())
                    {
                        int number = 1;

                        while (db.Playlists.Select((pl) => pl.PlaylistName).ToList().Contains(uniqueName))
                        {
                            number += 1;
                            uniqueName = name + " " + number;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not create DopamineContext. Exception: {0}", ex.Message);
                }
            });

            return uniqueName;
        }

        #endregion
    }
}
