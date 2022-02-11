using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
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

            foreach (var selectedTrack in selectedTracks)
            {
                var blacklistTrack = new BlacklistTrack();
                blacklistTrack.Path = selectedTrack.Path;
                blacklistTrack.SafePath = selectedTrack.SafePath;
                blacklistTrack.Artist = selectedTrack.ArtistName;
                blacklistTrack.Title = selectedTrack.TrackTitle;
                blacklistTracks.Add(blacklistTrack);
            }

            await this.blacklistTrackRepository.AddToBlacklistAsync(blacklistTracks);
            this.AddedTracksToBacklist(blacklistTracks.Count);
        }

        public async Task RemoveFromBlacklistAsync(BlacklistTrackViewModel blacklistTrack)
        {
            await this.blacklistTrackRepository.RemoveFromBlacklistAsync(blacklistTrack.BlacklistTrackId);
        }

        public async Task RemoveAllFromBlacklistAsync()
        {
            await this.blacklistTrackRepository.RemoveAllFromBlacklistAsync();
        }

        public async Task<bool> IsInBlacklistAsync(TrackViewModel track)
        {
            return await this.blacklistTrackRepository.IsInBlacklistAsync(track.SafePath);
        }
    }
}
