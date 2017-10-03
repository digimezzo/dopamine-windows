using Digimezzo.Utilities.Utils;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.IO;
using Dopamine.Common.Metadata;
using Dopamine.Common.Services.Cache;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Helpers;
using Dopamine.Core.Logging;
using System;
using System.Collections.Concurrent;
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
        #region Variables
        private ICacheService cacheService;
        private ILocalizationInfo info;
        private ITrackStatisticRepository trackStatisticRepository;
        private IList<string> files;
        private object lockObject = new object();
        private Timer addFilesTimer;
        private int addFilesTimeout = 250; // milliseconds
        private string instanceGuid;
        #endregion

        #region Construction
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
            this.addFilesTimer.Interval = this.addFilesTimeout;
            this.addFilesTimer.Elapsed += AddFilesTimerElapsedHandler;
            this.DeleteFileArtworkFromCacheAsync(this.instanceGuid);
        }
        #endregion

        #region IFileService
        public event TracksImportedHandler TracksImported = delegate { };

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
                        // The file is a directory: get the audio files in that directory.
                        List<string> audioFilePaths = this.ProcessDirectory(path);

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
            this.ProcessArgumentsAsync(args);
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
                CoreLogger.Current.Error("Error while creating Track from file '{0}'. Creating default track. Exception: {1}", path, ex.Message);
            }

            returnTrack.ArtistName = returnTrack.ArtistName.Replace(Defaults.UnknownArtistText, info.UnknownArtistText);
            returnTrack.AlbumArtist = returnTrack.AlbumArtist.Replace(Defaults.UnknownArtistText, info.UnknownArtistText);
            returnTrack.AlbumTitle = returnTrack.AlbumTitle.Replace(Defaults.UnknownAlbumText, info.UnknownAlbumText);
            returnTrack.GenreName = returnTrack.GenreName.Replace(Defaults.UnknownGenreText, info.UnknownGenreText);

            return returnTrack;
        }
        #endregion

        #region Private
        private async Task ProcessArgumentsAsync(string[] args)
        {
            if (args.Length > 1)
            {
                this.addFilesTimer.Stop();

                await Task.Run(() =>
                {
                    CoreLogger.Current.Info("Received commandline arguments.");

                    // Don't process index=0, as this contains the name of the executable.
                    for (int index = 1; index <= args.Length - 1; index++)
                    {
                        lock (this.lockObject)
                        {
                            this.files.Add(args[index]);
                            CoreLogger.Current.Info("Added file '{0}'", args[index]);
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
            if (EnvironmentUtils.IsSingleInstance(Core.Base.ProductInformation.ApplicationName))
            {
                lock (this.lockObject)
                {
                    CoreLogger.Current.Info("Finished adding files. Number of files added = {0}", this.files.Count.ToString());
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

            CoreLogger.Current.Info("Number of tracks to play = {0}", tracks.Count.ToString());

            if (tracks.Count > 0)
            {
                CoreLogger.Current.Info("Enqueuing {0} tracks.", tracks.Count.ToString());
                this.TracksImported(tracks);
            }
        }

        private List<string> ProcessPlaylistFile(string playlistPath)
        {
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult decodeResult = decoder.DecodePlaylist(playlistPath);

            if (!decodeResult.DecodeResult.Result)
            {
                CoreLogger.Current.Error("Error while decoding playlist file. Exception: {0}", decodeResult.DecodeResult.GetMessages());
            }

            return decodeResult.Paths;
        }

        private List<string> ProcessDirectory(string directoryPath)
        {
            List<string> paths = new List<string>();

            // Create a queue to hold exceptions that have occurred while scanning the directory tree
            var recurseExceptions = new ConcurrentQueue<Exception>();
            FileOperations.TryDirectoryRecursiveGetFiles(directoryPath, paths, FileFormats.SupportedMediaExtensions, recurseExceptions);

            if (recurseExceptions.Count > 0)
            {
                foreach (Exception recurseException in recurseExceptions)
                {
                    CoreLogger.Current.Error("Error while recursively getting files/folders. Exception: {0}", recurseException.ToString());
                }
            }

            return paths;
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
                    CoreLogger.Current.Error("There was a problem while fetching file artwork. Exception: {0}", ex.Message);
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
                            CoreLogger.Current.Error("There was a problem while deleting cached file artwork {0}. Exception: {1}", artworkFile, ex.Message);
                        }
                    }
                }
            });
        }
        #endregion
    }
}
