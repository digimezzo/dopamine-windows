using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Helpers;
using Dopamine.Common.IO;
using Dopamine.Common.Metadata;
using Dopamine.Common.Services.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Dopamine.Common.Services.File
{
    public class FileService : IFileService
    {
        private ICacheService cacheService;
        private ILocalizationInfo info;
        private ITrackStatisticRepository trackStatisticRepository;
        private IList<string> files;
        private object lockObject = new object();
        private Timer addFilesTimer;
        private int addFilesMilliseconds = 250;
        private string instanceGuid;

        public FileService(ICacheService cacheService, ITrackStatisticRepository trackStatisticRepository, ILocalizationInfo info)
        {
            this.cacheService = cacheService;
            this.trackStatisticRepository = trackStatisticRepository;
            this.info = info;

            // Unique identifier which will be used by this instance only to create cached artwork.
            // This prevents the cleanup function to delete artwork which is in use by this instance.
            this.instanceGuid = Guid.NewGuid().ToString();

            this.files = new List<string>();
            this.addFilesTimer = new Timer();
            this.addFilesTimer.Interval = this.addFilesMilliseconds;
            this.addFilesTimer.Elapsed += AddFilesTimerElapsedHandler;
            this.DeleteFileArtworkFromCacheAsync(this.instanceGuid);
        }

        public event TracksImportedHandler TracksImported = delegate { };
        public event EventHandler ImportingTracks = delegate { };

        public async Task<List<PlayableTrack>> ProcessFilesAsync(List<string> filenames)
        {
            var tracks = new List<PlayableTrack>();

            await Task.Run(async () =>
            {
                if (filenames == null) return;

                // Convert the files to tracks
                foreach (string path in filenames)
                {
                    if (FileFormats.IsSupportedAudioFile(path))
                    {
                        // The file is a supported audio format: add it directly.
                        tracks.Add(await this.CreateTrackAsync(path));

                        if (SettingsClient.Get<bool>("Behaviour", "EnqueueOtherFilesInFolder"))
                        {
                            // Get all files in the current (top) directory
                            List<string> audioFilePaths = this.ProcessDirectory(Path.GetDirectoryName(path), SearchOption.TopDirectoryOnly);

                            // Add all files from that directory
                            foreach (string audioFilePath in audioFilePaths)
                            {
                                if (!audioFilePath.Equals(path))
                                {
                                    tracks.Add(await this.CreateTrackAsync(audioFilePath));
                                }
                            }
                        }
                    }
                    else if (FileFormats.IsSupportedPlaylistFile(path))
                    {
                        // The file is a supported playlist format: process the contents of the playlist file.
                        List<string> audioFilePaths = this.ProcessPlaylistFile(path);

                        foreach (string audioFilePath in audioFilePaths)
                        {
                            tracks.Add(await this.CreateTrackAsync(audioFilePath));
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        // The file is a directory: get the audio files in that directory and its subdirectories.
                        List<string> audioFilePaths = this.ProcessDirectory(path, SearchOption.AllDirectories);

                        foreach (string audioFilePath in audioFilePaths)
                        {
                            tracks.Add(await this.CreateTrackAsync(audioFilePath));
                        }
                    }
                    else
                    {
                        // The file is unknown: do not process it.
                    }
                }
            });

            return tracks;
        }

        public void ProcessArguments(string[] args)
        {
            this.ImportTracks(args);
        }

        public async Task<PlayableTrack> CreateTrackAsync(string path)
        {
            var returnTrack = new PlayableTrack();

            try
            {
                var savedTrackStatistic = await this.trackStatisticRepository.GetTrackStatisticAsync(path);
                returnTrack = await MetadataUtils.Path2TrackAsync(path, savedTrackStatistic);
            }
            catch (Exception ex)
            {
                // Make sure the file can be opened by creating a Track with some default values
                returnTrack = PlayableTrack.CreateDefault(path);
                LogClient.Error("Error while creating Track from file '{0}'. Creating default track. Exception: {1}", path, ex.Message);
            }

            returnTrack.ArtistName = returnTrack.ArtistName.Replace(Defaults.UnknownArtistText, info.UnknownArtistText);
            returnTrack.AlbumArtist = returnTrack.AlbumArtist.Replace(Defaults.UnknownArtistText, info.UnknownArtistText);
            returnTrack.AlbumTitle = returnTrack.AlbumTitle.Replace(Defaults.UnknownAlbumText, info.UnknownAlbumText);
            returnTrack.GenreName = returnTrack.GenreName.Replace(Defaults.UnknownGenreText, info.UnknownGenreText);

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
                        lock (this.lockObject)
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
                lock (this.lockObject)
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
            List<string> tempFiles = null;

            await Task.Run(() =>
            {
                lock (this.lockObject)
                {
                    tempFiles = this.files.Select(item => (string)item.Clone()).ToList();
                    this.files.Clear(); // Clear the list
                }

                tempFiles.Sort(); // Sort the files alphabetically
            });

            List<PlayableTrack> tracks = await this.ProcessFilesAsync(tempFiles);

            LogClient.Info("Number of tracks to play = {0}", tracks.Count.ToString());

            if (tracks.Count > 0)
            {
                LogClient.Info("Enqueuing {0} tracks.", tracks.Count.ToString());
                this.TracksImported(tracks, tracks.First());
            }
        }

        private List<string> ProcessPlaylistFile(string playlistPath)
        {
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult decodeResult = decoder.DecodePlaylist(playlistPath);

            if (!decodeResult.DecodeResult.Result)
            {
                LogClient.Error("Error while decoding playlist file. Exception: {0}", decodeResult.DecodeResult.GetMessages());
            }

            return decodeResult.Paths;
        }

        private List<string> ProcessDirectory(string directoryPath, SearchOption searchOption)
        {
            var folderPaths = new List<FolderPathInfo>();

            try
            {
                folderPaths = FileOperations.GetValidFolderPaths(0, directoryPath, FileFormats.SupportedMediaExtensions, searchOption);
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while recursively getting files/folders for directory={0}. Exception: {1}", directoryPath, ex.Message);
            }

            // Ordering by path is required. Samba shares provide the files in the wrong order.
            return folderPaths != null && folderPaths.Count > 0 ? folderPaths.OrderBy(f => f.Path).Select(f => f.Path).ToList() : new List<string>();
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
