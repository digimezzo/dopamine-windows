using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Services.Entities
{
    public class TrackChildBase : BindableBase
    {
        public IList<long> TrackIds { get; } = new List<long>();

        public void AddTrackId(long trackId)
        {
            if (!this.TrackIds.Contains(trackId))
            {
                this.TrackIds.Add(trackId);
            }
        }
    }
}
