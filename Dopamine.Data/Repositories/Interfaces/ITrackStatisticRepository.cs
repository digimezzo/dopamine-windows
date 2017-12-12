using Dopamine.Data.Entities;
using System.Threading.Tasks;

namespace Dopamine.Data.Repositories.Interfaces
{
    public interface ITrackStatisticRepository
    {
        Task UpdateRatingAsync(string path, int rating);
        Task UpdateLoveAsync(string path, bool love);
        Task UpdateTrackStatisticAsync(TrackStatistic trackStatistic);
        Task<TrackStatistic> GetTrackStatisticAsync(string path);
    }
}
