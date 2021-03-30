using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Services.Metadata;
using Dopamine.Services.Playback;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Dopamine.Services.Lifetime
{
    public class LifetimeService : ILifetimeService
    {
        private ITerminationService cancellationService;
        private IPlaybackService playbackService;
        private IMetadataService metadataService;

        public LifetimeService(
            ITerminationService cancellationService,
            IPlaybackService playbackService,
            IMetadataService metadataService)
        {
            this.cancellationService = cancellationService;
            this.playbackService = playbackService;
            this.metadataService = metadataService;
        }

        public bool MustPerformClosingTasks { get => cancellationService.KeepRunning; }

        public async Task PerformClosingTasksAsync()
        {
            if(!cancellationService.Cancel())
            {
                return;
            }

            LogClient.Info("Performing closing tasks");

            // Write settings
            Stopwatch sw = Stopwatch.StartNew();
            SettingsClient.Write();
            LogClient.Info($"Write settings. Time required: {sw.ElapsedMilliseconds} ms");

            // Save queued tracks
            sw.Restart();

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

            LogClient.Info($"Save queued tracks. Time required: {sw.ElapsedMilliseconds} ms");

            // Stop playing
            sw.Restart();
            this.playbackService.Stop();
            LogClient.Info($"Stop playback. Time required: {sw.ElapsedMilliseconds} ms");

            // Update file metadata
            sw.Restart();
            await this.metadataService.ForceSaveFileMetadataAsync();
            LogClient.Info($"Update file metadata. Time required: {sw.ElapsedMilliseconds} ms");

            // Save playback counters
            sw.Restart();

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

            LogClient.Info($"Save playback counters. Time required: {sw.ElapsedMilliseconds} ms");
        }
    }
}
