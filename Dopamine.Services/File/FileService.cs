using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.IO;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Data.Repositories;
using Dopamine.Services.Cache;
using Dopamine.Services.Entities;
using Dopamine.Services.Extensions;
using Dopamine.Services.Lifetime;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Dopamine.Services.File
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FileService : IFileService
    {
        private ICacheService cacheService;
        private ITerminationService cancellationService;
        private ITrackRepository trackRepository;
        private IContainerProvider container;
        private IList<string> files = new List<string>();
        private Timer addFilesTimer;
        private int addFilesMilliseconds = 250;
        private string instanceGuid;

        public FileService(
            ICacheService cacheService,
            ITerminationService cancellationService,
            ITrackRepository trackRepository,
            IContainerProvider container)
        {
            this.cacheService = cacheService;
            this.cancellationService = cancellationService;
            this.trackRepository = trackRepository;
            this.container = container;

            // Unique identifier which will be used by this instance only to create cached artwork.
            // This prevents the cleanup function to delete artwork which is in use by this instance.
            this.instanceGuid = Guid.NewGuid().ToString();

            this.addFilesTimer = new Timer();
            this.addFilesTimer.Interval = this.addFilesMilliseconds;
            this.addFilesTimer.Elapsed += AddFilesTimerElapsedHandler;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DeleteFileArtworkFromCacheAsync(this.instanceGuid);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public event TracksImportedHandler TracksImported = delegate { };
        public event EventHandler ImportingTracks = delegate { };

        private async Task<Tuple<List<TrackViewModel>, TrackViewModel>> ProcessFileAsync(string path)
        {
            var tracks = new List<TrackViewModel>();
            TrackViewModel selectedTrack = await this.CreateTrackAsync(path);

            tracks.Add(await this.CreateTrackAsync(path));

            return new Tuple<List<TrackViewModel>, TrackViewModel>(tracks, selectedTrack);
        }

        public async Task<IList<TrackViewModel>> ProcessFilesInDirectoryAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return new List<TrackViewModel>();
            }

            string[] paths = Directory.GetFiles(directoryPath).SortNaturally().ToArray();

            var tracks = new List<TrackViewModel>();

            await Task.Run(async () =>
            {
                foreach (string path in paths)
                {
                    if (FileFormats.IsSupportedAudioFile(path))
                    {
                        tracks.Add(await this.CreateTrackAsync(path));
                    }
                }
            });

            return tracks;
        }

        public async Task<IList<TrackViewModel>> ProcessFilesAsync(IList<string> paths, bool processPlaylistFiles)
        {
            var tracks = new List<TrackViewModel>();

            if (paths == null)
            {
                return tracks;
            }

            // Convert the files to tracks
            foreach (string path in paths)
            {
                if (FileFormats.IsSupportedAudioFile(path))
                {
                    // The file is a supported audio format: add it directly.
                    tracks.Add(await this.CreateTrackAsync(path));
                }
                else if (processPlaylistFiles && FileFormats.IsSupportedStaticPlaylistFile(path))
                {
                    // The file is a supported playlist format: process the contents of the playlist file.
                    foreach (string audioFilePath in this.ProcessPlaylistFile(path))
                    {
                        tracks.Add(await this.CreateTrackAsync(audioFilePath));
                    }
                }
                else if (Directory.Exists(path))
                {
                    // The file is a directory: get the audio files in that directory and all its sub directories.
                    foreach (string audioFilePath in await this.ProcessDirectoryAsync(path))
                    {
                        tracks.Add(await this.CreateTrackAsync(audioFilePath));
                    }
                }
                else
                {
                    // The file is unknown: do not process it.
                }
            }

            return tracks;
        }

        public void ProcessArguments(string[] args)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.ImportTracks(args);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task<TrackViewModel> CreateTrackAsync(string path)
        {
            TrackViewModel returnTrack = null;

            try
            {
                Track track = await MetadataUtils.Path2TrackAsync(path);

                returnTrack = container.ResolveTrackViewModel(track);
            }
            catch (Exception ex)
            {
                // Make sure the file can be opened by creating a Track with some default values
                returnTrack = container.ResolveTrackViewModel(Track.CreateDefault(path));
                LogClient.Error("Error while creating Track from file '{0}'. Creating default track. Exception: {1}", path, ex.Message);
            }

            return returnTrack;
        }

        private async Task ImportTracks(string[] args)
        {
            if (args.Length > 1)
            {
                this.addFilesTimer.Stop();
                this.ImportingTracks(this, new EventArgs());

                await Task.Run(() =>
                {
                    LogClient.Info("Received commandline arguments.");

                    // Don't process index=0, as this contains the name of the executable.
                    for (int index = 1; index <= args.Length - 1; index++)
                    {
                        lock (this.files)
                        {
                            this.files.Add(args[index]);
                            LogClient.Info("Added file '{0}'", args[index]);
                        }
                    }
                });

                this.RestartAddFilesTimer();
            }
        }

        private void RestartAddFilesTimer()
        {
            this.addFilesTimer.Stop();
            this.addFilesTimer.Start();
        }

        private async void AddFilesTimerElapsedHandler(Object sender, ElapsedEventArgs e)
        {
            this.addFilesTimer.Stop();

            // Check if there is only 1 instance (this one) of the application running. If not,
            // that could mean there are other instances trying to send files to this instance.
            if (EnvironmentUtils.IsSingleInstance(ProductInformation.ApplicationName))
            {
                lock (this.files)
                {
                    LogClient.Info("Finished adding files. Number of files added = {0}", this.files.Count.ToString());
                }

                await Application.Current.Dispatcher.BeginInvoke(new Action(async () => await this.ImportFilesAsync()));
            }
            else
            {
                // There are still other instances trying to send files. Check again next time.
                this.RestartAddFilesTimer();
            }
        }

        private async Task ImportFilesAsync()
        {
            try
            {
                List<string> tempFiles = null;

                await Task.Run(() =>
                {
                    lock (this.files)
                    {
                        // Sort the files in a natural way and clone the list
                        tempFiles = this.files.OrderByAlphaNumeric(item => item).Select(item => (string)item.Clone()).ToList();
                        this.files.Clear(); // Clear the list
                    }
                });

                IList<TrackViewModel> tracks = await this.ProcessFilesAsync(tempFiles, true);
                TrackViewModel selectedTrack = tracks.First();

                LogClient.Info("Number of tracks to play = {0}", tracks.Count.ToString());

                if (tracks.Count > 0)
                {
                    LogClient.Info("Enqueuing {0} tracks.", tracks.Count.ToString());
                    this.TracksImported(tracks, selectedTrack);
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not enqueue tracks. Exception: {0}", ex.Message);
            }
        }

        private IList<string> ProcessPlaylistFile(string playlistPath)
        {
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult decodeResult = decoder.DecodePlaylist(playlistPath);

            if (!decodeResult.DecodeResult.Result)
            {
                LogClient.Error("Error while decoding playlist file. Exception: {0}", decodeResult.DecodeResult.GetMessages());
            }

            return decodeResult.Paths;
        }

        private async Task<List<string>> ProcessDirectoryAsync(string directoryPath)
        {
            var folderPaths = new List<FolderPathInfo>();

            try
            {
                folderPaths = await FileOperations.GetValidFolderPathsAsync(
                    0,
                    directoryPath,
                    FileFormats.SupportedMediaExtensions,
                    cancellationService.CancellationToken);
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while recursively getting files/folders for directory={0}. Exception: {1}", directoryPath, ex.Message);
            }

            // Sort the files in a natural way
            return folderPaths != null && folderPaths.Count > 0 ? folderPaths.OrderByAlphaNumeric(f => f.Path).Select(f => f.Path).ToList() : new List<string>();
        }

        private async Task DeleteFileArtworkFromCacheAsync(string exclude)
        {
            await Task.Run(() =>
            {
                string[] artworkFiles = null;

                try
                {
                    if (System.IO.Directory.Exists(this.cacheService.CoverArtCacheFolderPath))
                    {
                        artworkFiles = System.IO.Directory.GetFiles(this.cacheService.CoverArtCacheFolderPath, "file-*.jpg");
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("There was a problem while fetching file artwork. Exception: {0}", ex.Message);
                }

                if (artworkFiles != null && artworkFiles.Count() > 0)
                {

                    foreach (string artworkFile in artworkFiles)
                    {
                        try
                        {
                            // Do not delete file from this instance
                            if (!artworkFile.StartsWith("file-" + this.instanceGuid))
                            {
                                System.IO.File.Delete(artworkFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogClient.Error("There was a problem while deleting cached file artwork {0}. Exception: {1}", artworkFile, ex.Message);
                        }
                    }
                }
            });
        }
    }
}
