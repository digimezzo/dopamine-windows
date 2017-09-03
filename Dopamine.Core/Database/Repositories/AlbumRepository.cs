using Dopamine.Core.Database.Entities;
using Dopamine.Core.Database.Repositories.Interfaces;
using Dopamine.Core.Helpers;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Repositories
{
    public abstract class AlbumRepository : IAlbumRepository
    {
        #region Variables
        private ISQLiteConnectionFactory factory;
        private ILocalizationInfo info;
        #endregion

        #region Properties
        public ISQLiteConnectionFactory Factory => this.factory;
        #endregion

        #region Construction
        public AlbumRepository(ISQLiteConnectionFactory factory, ILocalizationInfo info)
        {
            this.factory = factory;
            this.info = info;
        }
        #endregion

        #region IAlbumRepository
        public async Task DeleteOrphanedAlbumsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = this.factory.GetConnection())
                    {
                        try
                        {
                            conn.Execute("DELETE FROM Album WHERE AlbumID NOT IN (SELECT AlbumID FROM Track);");
                        }
                        catch (Exception ex)
                        {
                            CoreLogger.Current.Error("There was a problem while deleting orphaned Albums. Exception: {0}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CoreLogger.Current.Error("Could not connect to the database. Exception: {0}", ex.Message);
                }
            });
        }
       
        public abstract Task<Album> AddAlbumAsync(Album album);

        public abstract Album GetAlbum(long albumID);

        public abstract Task<Album> GetAlbumAsync(string albumTitle, string albumArtist);

        public abstract Task<List<Album>> GetAlbumsAsync();

        public abstract Task<List<Album>> GetAlbumsAsync(IList<Artist> artists);

        public abstract Task<List<Album>> GetAlbumsAsync(IList<Genre> genres);

        public abstract Task<List<Album>> GetFrequentAlbumsAsync(int limit);

        public abstract Task<bool> UpdateAlbumArtworkAsync(string albumTitle, string albumArtist, string artworkID);

        public abstract Task<bool> UpdateAlbumAsync(Album album);
        #endregion
    }
}
