using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Services.Blacklist
{
    public class BlacklistService : IBlacklistService
    {
        private readonly IBlacklistTrackRepository blacklistTrackRepository;

        public BlacklistService(IBlacklistTrackRepository blacklistTrackRepository)
        {
            this.blacklistTrackRepository = blacklistTrackRepository;
        }

        public event Action<int> AddedTracksToBacklist = delegate { };

        public async Task AddToBlacklistAsync(IList<TrackViewModel> selectedTracks)
        {
            IList<BlacklistTrack> blacklistTracks = new List<BlacklistTrack>();

            foreach (TrackViewModel selectedTrack in selectedTracks)
            {
                if (!await this.IsInBlacklistAsync(selectedTrack))
                {
                    var blacklistTrack = new BlacklistTrack();
                    blacklistTrack.Artist = selectedTrack.ArtistName;
                    blacklistTrack.Title = selectedTrack.TrackTitle;
                    blacklistTrack.Path = selectedTrack.Path;
                    blacklistTrack.SafePath = selectedTrack.SafePath;
                    blacklistTracks.Add(blacklistTrack);
                }
            }

            if(blacklistTracks.Count > 0)
            {
                await this.blacklistTrackRepository.AddToBlacklistAsync(blacklistTracks);
                this.AddedTracksToBacklist(blacklistTracks.Count);
            }
        }

        public async Task RemoveFromBlacklistAsync(long blacklistTrackId)
        {
            await this.blacklistTrackRepository.RemoveFromBlacklistAsync(blacklistTrackId);
        }

        public async Task RemoveAllFromBlacklistAsync()
        {
            await this.blacklistTrackRepository.RemoveAllFromBlacklistAsync();
        }

        public async Task<bool> IsInBlacklistAsync(TrackViewModel track)
        {
            return await this.blacklistTrackRepository.IsInBlacklistAsync(track.SafePath);
        }

        public async Task<IList<BlacklistTrackViewModel>> GetBlacklistTracksAsync()
        {
            IList<BlacklistTrack> blacklistTracks = await this.blacklistTrackRepository.GetBlacklistTracksAsync();
            IList<BlacklistTrackViewModel> blacklistTrackViewModels = new List<BlacklistTrackViewModel>(blacklistTracks.OrderBy(x => x.Artist).ThenBy(x => x.Title).ThenBy(x => x.Path).Select(x => new BlacklistTrackViewModel(x)));

            return blacklistTrackViewModels;
        }
    }
}
