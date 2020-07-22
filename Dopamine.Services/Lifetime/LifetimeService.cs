using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Alex; //Digimezzo.Foundation.Core.Settings
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using System;
using System.Threading.Tasks;

namespace Dopamine.Services.Lifetime
{
    public class LifetimeService : ILifetimeService
    {
        private IPlaybackService playbackService;
        private IMetadataService metadataService;

        public bool MustPerformClosingTasks { get; private set; } = true;

        public LifetimeService(IPlaybackService playbackService, IMetadataService metadataService)
        {
            this.playbackService = playbackService;
            this.metadataService = metadataService;
        }

        public async Task PerformClosingTasksAsync()
        {
            LogClient.Info("Performing closing tasks");

            // Write settings
            DateTime startTime = DateTime.Now;
            SettingsClient.Write();
            LogClient.Info($"Write settings. Time required: {Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds)} ms");

            // Save queued tracks
            startTime = DateTime.Now;

            if (this.playbackService.IsSavingQueuedTracks)
            {
                while (this.playbackService.IsSavingQueuedTracks)
                {
                    await Task.Delay(50);
                }
            }
            else
            {
                await this.playbackService.SaveQueuedTracksAsync();
            }

            LogClient.Info($"Save queued tracks. Time required: {Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds)} ms");

            // Stop playing
            startTime = DateTime.Now;
            this.playbackService.Stop();
            LogClient.Info($"Stop playback. Time required: {Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds)} ms");

            // Update file metadata
            startTime = DateTime.Now;
            await this.metadataService.ForceSaveFileMetadataAsync();
            LogClient.Info($"Update file metadata. Time required: {Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds)} ms");


            // Save playback counters
            startTime = DateTime.Now;
            
            if (this.playbackService.IsSavingPlaybackCounters)
            {
                while (this.playbackService.IsSavingPlaybackCounters)
                {
                    await Task.Delay(50);
                }
            }
            else
            {
                await this.playbackService.SavePlaybackCountersAsync();
            }

            LogClient.Info($"Save playback counters. Time required: {Convert.ToInt64(DateTime.Now.Subtract(startTime).TotalMilliseconds)} ms");

            this.MustPerformClosingTasks = false;
        }
    }
}
