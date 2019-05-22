using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Extensions;
using Dopamine.Data.Metadata;
using Dopamine.Data.Repositories;
using Dopamine.Services.Playback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Dopamine.Services.Metadata
{
    public class FileMetadataUpdater
    {
        private IPlaybackService playbackService;
        private ITrackRepository trackRepository;
        private Timer updateFileMetadataTimer = new Timer();
        private int updateMetadataShortTimeout = 50; // 50 milliseconds
        private int updateMetadataLongTimeout = 15000; // 15 seconds
        private Dictionary<string, FileMetadata> fileMetadataToUpdate = new Dictionary<string, FileMetadata>();
        private object fileMetadataToUpdateLock = new object();
        private bool isUpdatingFileMetadata;

        public FileMetadataUpdater(IPlaybackService playbackService, ITrackRepository trackRepository)
        {
            this.playbackService = playbackService;
            this.trackRepository = trackRepository;

            this.updateFileMetadataTimer.Interval = this.updateMetadataLongTimeout;
            this.updateFileMetadataTimer.Elapsed += async (_, __) => await this.UpdateFileMetadataAsync(true);
            this.playbackService.PlaybackStopped += async (_, __) => await this.UpdateFileMetadataAsync(true);
            this.playbackService.PlaybackFailed += async (_, __) => await this.UpdateFileMetadataAsync(true);
            this.playbackService.PlaybackSuccess += async (_, __) => await this.UpdateFileMetadataAsync(true);
        }

        public async Task UpdateFileMetadataAsync(IList<FileMetadata> fileMetadatas)
        {
            this.updateFileMetadataTimer.Stop();

            await Task.Run(() =>
            {
                lock (this.fileMetadataToUpdateLock)
                {
                    foreach (FileMetadata fmd in fileMetadatas)
                    {
                        if (this.fileMetadataToUpdate.ContainsKey(fmd.SafePath))
                        {
                            this.fileMetadataToUpdate[fmd.SafePath] = fmd;
                        }
                        else
                        {
                            this.fileMetadataToUpdate.Add(fmd.SafePath, fmd);
                        }
                    }
                }
            });

            // The next time, almost don't wait.
            this.updateFileMetadataTimer.Interval = this.updateMetadataShortTimeout; 
            this.updateFileMetadataTimer.Start();
        }

        public FileMetadata GetFileMetadataToUpdate(string path)
        {
            bool mustStartTimer = this.updateFileMetadataTimer.Enabled;
            this.updateFileMetadataTimer.Stop();

            FileMetadata fileMetadata = null;

            lock (fileMetadataToUpdateLock)
            {
                if (this.fileMetadataToUpdate.ContainsKey(path.ToSafePath()))
                {
                    fileMetadata = this.fileMetadataToUpdate[path.ToSafePath()];
                }
            }

            if (mustStartTimer)
            {
                this.updateFileMetadataTimer.Start();
            }

            return fileMetadata;
        }

        public async Task ForceUpdateFileMetadataAsync()
        {
            if (this.isUpdatingFileMetadata)
            {
                while (this.isUpdatingFileMetadata)
                {
                    await Task.Delay(50);
                }
            }

            // In case the previous loop didn't save all metadata to files, force it again.
            await this.UpdateFileMetadataAsync(false);
        }

        private async Task UpdateFileMetadataAsync(bool canRetry)
        {
            bool mustStartTimer = false;
            this.updateFileMetadataTimer.Stop();
            this.isUpdatingFileMetadata = true;

            var filesToSync = new List<FileMetadata>();

            await Task.Run(() =>
            {
                lock (fileMetadataToUpdateLock)
                {
                    int numberToProcess = this.fileMetadataToUpdate.Count;

                    if (numberToProcess == 0)
                    {
                        return;
                    }

                    while (numberToProcess > 0)
                    {
                        FileMetadata fmd = this.fileMetadataToUpdate.First().Value;
                        numberToProcess--;

                        try
                        {
                            fmd.Save();
                            filesToSync.Add(fmd);
                            this.fileMetadataToUpdate.Remove(fmd.SafePath);
                        }
                        catch (IOException ex)
                        {
                            LogClient.Error("Unable to save metadata to the file for Track '{0}'. The file is probably playing. Trying again in {1} seconds. Exception: {2}", fmd.SafePath, this.updateMetadataLongTimeout / 1000, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("Unable to save metadata to the file for Track '{0}'. Not trying again. Exception: {1}", fmd.SafePath, ex.Message);
                            this.fileMetadataToUpdate.Remove(fmd.SafePath);
                        }
                    }

                    // If there are still queued FileMetadata's, start the timer.
                    if (this.fileMetadataToUpdate.Count > 0)
                    {
                        mustStartTimer = true;
                    }
                }
            });

            // Sync file size and last modified date in the database
            foreach (FileMetadata fmd in filesToSync)
            {
                await this.trackRepository.UpdateTrackFileInformationAsync(fmd.SafePath);
            }

            // The next time, wait longer.
            this.updateFileMetadataTimer.Interval = this.updateMetadataLongTimeout; 

            this.isUpdatingFileMetadata = false;

            if (canRetry && mustStartTimer)
            {
                this.updateFileMetadataTimer.Start();
            }
        }
    }
}
