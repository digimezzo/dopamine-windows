using Dopamine.Common.Database.Entities;
using System.Threading.Tasks;

namespace Dopamine.Common.Database.Repositories.Interfaces
{
    public interface ITrackStatisticRepository
    {
        Task UpdateRatingAsync(string path, int rating);
        Task UpdateLoveAsync(string path, bool love);
        Task UpdateCountersAsync(string path, int playCount, int skipCount, long dateLastPlayed);
        Task<TrackStatistic> GetTrackStatisticAsync(string path);
    }
}
