using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Helpers;

namespace Dopamine.UWP.Database.Repositories
{
    public class ArtistRepository : Core.Database.Repositories.ArtistRepository
    {
        #region Construction
        public ArtistRepository(ISQLiteConnectionFactory factory, ILocalizationInfo info) : base(factory, info)
        {
        }
        #endregion

        #region Overrides
        public override Task<Artist> AddArtistAsync(Artist artist)
        {
            throw new NotImplementedException();
        }

        public override Task<Artist> GetArtistAsync(string artistName)
        {
            throw new NotImplementedException();
        }

        public override Task<List<Artist>> GetArtistsAsync(ArtistOrder artistOrder)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
