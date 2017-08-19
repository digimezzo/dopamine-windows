using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dopamine.Core.Database.Entities;

namespace Dopamine.UWP.Database.Repositories
{
    public class AlbumRepository : Core.Database.Repositories.AlbumRepository
    {
        #region Overrides
        public override Task<Album> AddAlbumAsync(Album album)
        {
            throw new NotImplementedException();
        }

        public override Album GetAlbum(long albumID)
        {
            throw new NotImplementedException();
        }

        public override Task<Album> GetAlbumAsync(string albumTitle, string albumArtist)
        {
            throw new NotImplementedException();
        }

        public override Task<List<Album>> GetAlbumsAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<List<Album>> GetAlbumsAsync(IList<Artist> artists)
        {
            throw new NotImplementedException();
        }

        public override Task<List<Album>> GetAlbumsAsync(IList<Genre> genres)
        {
            throw new NotImplementedException();
        }

        public override Task<List<Album>> GetFrequentAlbumsAsync(int limit)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> UpdateAlbumArtworkAsync(string albumTitle, string albumArtist, string artworkID)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> UpdateAlbumAsync(Album album)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
