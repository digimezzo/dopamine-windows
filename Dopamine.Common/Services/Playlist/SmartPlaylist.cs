using System.Collections.Generic;

namespace Dopamine.Common.Services.Playlist
{
    public class SmartPlaylist
    {
        #region Properties
        public string Name { get; set; }
        public List<Predicate> Predicates { get; set; }
        #endregion
    }
}