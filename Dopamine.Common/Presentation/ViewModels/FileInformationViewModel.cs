using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Database;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Metadata;
using Dopamine.Common.Utils;
using Prism.Mvvm;
using System;
using System.Globalization;
using System.Windows;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class FileInformationViewModel : BindableBase
    {
        #region Variables
        private IMetadataService metaDataService;

        // Song
        private string songTitle;
        private string songArtists;
        private string songAlbum;
        private string songYear;
        private string songGenres;
        private string songTrackNumber;

        // File
        private string fileName;
        private string fileFolder;
        private string filePath;
        private string fileSize;
        private string fileLastModified;

        // Audio
        private string audioDuration;
        private string audioType;
        private string audioSampleRate;
        private string audioBitrate;
        #endregion

        #region Properties
        public string SongTitle
        {
            get { return this.songTitle; }
            set { SetProperty<string>(ref this.songTitle, value); }
        }

        public string SongArtists
        {
            get { return this.songArtists; }
            set { SetProperty<string>(ref this.songArtists, value); }
        }

        public string SongAlbum
        {
            get { return this.songAlbum; }
            set { SetProperty<string>(ref this.songAlbum, value); }
        }

        public string SongYear
        {
            get { return this.songYear; }
            set { SetProperty<string>(ref this.songYear, value); }
        }

        public string SongGenres
        {
            get { return this.songGenres; }
            set { SetProperty<string>(ref this.songGenres, value); }
        }

        public string SongTrackNumber
        {
            get { return this.songTrackNumber; }
            set { SetProperty<string>(ref this.songTrackNumber, value); }
        }

        public string FileName
        {
            get { return this.fileName; }
            set { SetProperty<string>(ref this.fileName, value); }
        }

        public string FileFolder
        {
            get { return this.fileFolder; }
            set { SetProperty<string>(ref this.fileFolder, value); }
        }

        public string FilePath
        {
            get { return this.filePath; }
            set { SetProperty<string>(ref this.filePath, value); }
        }

        public string FileSize
        {
            get { return this.fileSize; }
            set { SetProperty<string>(ref this.fileSize, value); }
        }

        public string FileLastModified
        {
            get { return this.fileLastModified; }
            set { SetProperty<string>(ref this.fileLastModified, value); }
        }

        public string AudioDuration
        {
            get { return this.audioDuration; }
            set { SetProperty<string>(ref this.audioDuration, value); }
        }


        public string AudioType
        {
            get { return this.audioType; }
            set { SetProperty<string>(ref this.audioType, value); }
        }

        public string AudioSampleRate
        {
            get { return this.audioSampleRate; }
            set { SetProperty<string>(ref this.audioSampleRate, value); }
        }

        public string AudioBitrate
        {
            get { return this.audioBitrate; }
            set { SetProperty<string>(ref this.audioBitrate, value); }
        }
        #endregion

        #region Construction
        public FileInformationViewModel(IMetadataService metaDataService, MergedTrack selectedTrack)
        {
            this.metaDataService = metaDataService;

            this.GetFileMetadata(selectedTrack);
            this.GetFileInformation(selectedTrack);
        }
        #endregion

        #region Private
        private void GetFileMetadata(MergedTrack selectedTrack)
        {
            try
            {
                var fm = new FileMetadata(selectedTrack.Path);

                this.SongTitle = fm.Title.Value;
                this.SongAlbum = fm.Album.Value;
                this.SongArtists = string.Join(", ", fm.Artists.Values);
                this.SongGenres = string.Join(", ", fm.Genres.Values);
                this.SongYear = fm.Year.Value.ToString();
                this.SongTrackNumber = fm.TrackNumber.Value.ToString();
                this.AudioDuration = FormatUtils.FormatTime(fm.Duration);
                this.AudioType = fm.Type;
                this.AudioSampleRate = string.Format("{0} {1}", fm.SampleRate.ToString(), "Hz");
                this.AudioBitrate = string.Format("{0} {1}", fm.BitRate.ToString(), "kbps");
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while getting file Metadata. Exception: {0}", ex.Message);
            }
        }

        private void GetFileInformation(MergedTrack selectedTrack)
        {
            try
            {
                this.FileName = FileUtils.Name(selectedTrack.Path);
                this.FileFolder = FileUtils.Folder(selectedTrack.Path);
                this.FilePath = selectedTrack.Path;
                this.FileSize = FormatUtils.FormatFileSize(FileUtils.SizeInBytes(selectedTrack.Path));
                this.FileLastModified = FileUtils.DateModified(selectedTrack.Path).ToString("D", new CultureInfo(Application.Current.FindResource("Language_ISO639-1")?.ToString()));
            }
            catch (Exception ex)
            {
                LogClient.Error("Error while getting file Information. Exception: {0}", ex.Message);
            }
        }
        #endregion
    }
}
