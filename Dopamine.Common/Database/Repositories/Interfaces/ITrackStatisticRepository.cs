using Dopamine.Core.Database.Entities;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface ITrackStatisticRepository
    {
        Task UpdateRatingAsync(string path, int rating);
        Task UpdateLoveAsync(string path, bool love);
        Task UpdateTrackStatisticAsync(TrackStatistic trackStatistic);
        Task<TrackStatistic> GetTrackStatisticAsync(string path);
    }
}
