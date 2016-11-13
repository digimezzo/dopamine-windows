using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Metadata;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Metadata;
using Dopamine.Core.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class EditTrackViewModel : BindableBase
    {
        #region Variables
        private bool isBusy;
        private IList<TrackInfo> trackInfos;
        private IMetadataService metadataService;
        private IDialogService dialogService;

        private string multipleValuesText;
        private bool hasMultipleArtwork;
        private string artworkSize;

        private bool updateAlbumArtwork;
        private MetadataValue artists;
        private MetadataValue title;
        private MetadataValue album;
        private MetadataValue albumArtists;
        private MetadataValue year;
        private MetadataValue trackNumber;
        private MetadataValue trackCount;
        private MetadataValue discNumber;
        private MetadataValue discCount;
        private MetadataValue genres;
        private MetadataValue grouping;
        private MetadataValue comment;
        private MetadataValue lyrics;
        private MetadataArtworkValue artwork;
        private BitmapImage artworkThumbnail;

        private int previousIndex = 0;
        private int contentSlideInFrom;
        private UserControl editTrackContent;
        #endregion

        #region Commands
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand ChangeArtworkCommand { get; set; }
        public DelegateCommand RemoveArtworkCommand { get; set; }
        #endregion

        #region Readonly Properties
        public string DialogTitle
        {
            get
            {
                string dialogTitle = this.trackInfos.Count > 1 ? ResourceUtils.GetStringResource("Language_Edit_Multiple_Songs") : ResourceUtils.GetStringResource("Language_Edit_Song");
                return dialogTitle.ToLower();
            }
        }

        public string MultipleTracksWarningText
        {
            get { return ResourceUtils.GetStringResource("Language_Multiple_Songs_Selected").Replace("%trackcount%", this.trackInfos.Count.ToString()); }
        }

        public bool ShowMultipleTracksWarning
        {
            get { return this.trackInfos.Count > 1; }
        }

        public bool HasArtwork
        {
            get { return this.Artwork != null && this.Artwork.Value != null; }
        }
        #endregion

        #region Properties
        public int ContentSlideInFrom
        {
            get { return this.contentSlideInFrom; }
            set { SetProperty<int>(ref this.contentSlideInFrom, value); }
        }

        public UserControl EditTrackContent
        {
            get { return this.editTrackContent; }
            set { SetProperty<UserControl>(ref this.editTrackContent, value); }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set { SetProperty<bool>(ref this.isBusy, value); }
        }

        public bool HasMultipleArtwork
        {
            get { return this.hasMultipleArtwork; }
            set { SetProperty<bool>(ref this.hasMultipleArtwork, value); }
        }

        public BitmapImage ArtworkThumbnail
        {
            get { return this.artworkThumbnail; }
            set { SetProperty<BitmapImage>(ref this.artworkThumbnail, value); }
        }

        public MetadataArtworkValue Artwork
        {
            get { return this.artwork; }
            set { SetProperty<MetadataArtworkValue>(ref this.artwork, value); }
        }

        public MetadataValue Artists
        {
            get { return this.artists; }
            set { SetProperty<MetadataValue>(ref this.artists, value); }
        }

        public MetadataValue Title
        {
            get { return this.title; }
            set { SetProperty<MetadataValue>(ref this.title, value); }
        }

        public MetadataValue Album
        {
            get { return this.album; }
            set { SetProperty<MetadataValue>(ref this.album, value); }
        }

        public MetadataValue AlbumArtists
        {
            get { return this.albumArtists; }
            set { SetProperty<MetadataValue>(ref this.albumArtists, value); }
        }

        public MetadataValue Year
        {
            get { return this.year; }
            set { SetProperty<MetadataValue>(ref this.year, value); }
        }

        public MetadataValue TrackNumber
        {
            get { return this.trackNumber; }
            set { SetProperty<MetadataValue>(ref this.trackNumber, value); }
        }

        public MetadataValue TrackCount
        {
            get { return this.trackCount; }
            set { SetProperty<MetadataValue>(ref this.trackCount, value); }
        }

        public MetadataValue DiscNumber
        {
            get { return this.discNumber; }
            set { SetProperty<MetadataValue>(ref this.discNumber, value); }
        }

        public MetadataValue DiscCount
        {
            get { return this.discCount; }
            set { SetProperty<MetadataValue>(ref this.discCount, value); }
        }

        public MetadataValue Genres
        {
            get { return this.genres; }
            set { SetProperty<MetadataValue>(ref this.genres, value); }
        }

        public MetadataValue Grouping
        {
            get { return this.grouping; }
            set { SetProperty<MetadataValue>(ref this.grouping, value); }
        }

        public MetadataValue Comment
        {
            get { return this.comment; }
            set { SetProperty<MetadataValue>(ref this.comment, value); }
        }

        public MetadataValue Lyrics
        {
            get { return this.lyrics; }
            set { SetProperty<MetadataValue>(ref this.lyrics, value); }
        }

        public string ArtworkSize
        {
            get { return this.artworkSize; }
            set { SetProperty<string>(ref this.artworkSize, value); }
        }

        public bool UpdateAlbumArtwork
        {
            get { return this.updateAlbumArtwork; }
            set { SetProperty<bool>(ref this.updateAlbumArtwork, value); }
        }
        #endregion

        #region Construction
        public EditTrackViewModel(IList<TrackInfo> trackinfos, IMetadataService metadataService, IDialogService dialogService)
        {
            this.multipleValuesText = ResourceUtils.GetStringResource("Language_Multiple_Values");

            this.artists = new MetadataValue();
            this.title = new MetadataValue();
            this.album = new MetadataValue();
            this.albumArtists = new MetadataValue();
            this.year = new MetadataValue();
            this.trackNumber = new MetadataValue();
            this.trackCount = new MetadataValue();
            this.discNumber = new MetadataValue();
            this.discCount = new MetadataValue();
            this.genres = new MetadataValue();
            this.grouping = new MetadataValue();
            this.comment = new MetadataValue();
            this.lyrics = new MetadataValue();
            this.artwork = new MetadataArtworkValue();

            this.trackInfos = trackinfos;
            this.metadataService = metadataService;
            this.dialogService = dialogService;

            this.HasMultipleArtwork = false;
            this.UpdateAlbumArtwork = false;

            this.LoadedCommand = new DelegateCommand(async () => await this.GetFilesMetadataAsync());

            this.NavigateCommand = new DelegateCommand<string>((index) => Navigate(index));

            this.ChangeArtworkCommand = new DelegateCommand(async () =>
            {
                if (!await OpenFileUtils.OpenImageFileAsync(new Action<byte[]>(this.UpdateArtwork)))
                {
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Changing_Image"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                }
            });

            this.RemoveArtworkCommand = new DelegateCommand(() => this.UpdateArtwork(null));

            this.Navigate("1"); // Make sure something is displayed when the screen is shown
        }
        #endregion

        #region Private
        private void Navigate(string indexString)
        {
            if (string.IsNullOrWhiteSpace(indexString))
                return;

            int index = 0;

            int.TryParse(indexString, out index);

            if (index == 0)
                return;

            this.ContentSlideInFrom = index <= this.previousIndex ? -30 : 30;

            this.previousIndex = index;

            if (index == 1)
            {
                // Tags
                var content = new EditTrackTagsControl();
                content.DataContext = this;
                this.EditTrackContent = content;
            }
            else if (index == 2)
            {
                // Lyrics
                var content = new EditTrackLyricsControl();
                content.DataContext = this;
                this.EditTrackContent = content;
            }
        }

        private async Task GetFilesMetadataAsync()
        {
            List<FileMetadata> fileMetadatas = new List<FileMetadata>();

            try
            {
                foreach (TrackInfo ti in this.trackInfos)
                {
                    fileMetadatas.Add(await this.metadataService.GetFileMetadataAsync(ti.Path));
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("An error occured while getting the metadata from the files. Exception: {0}", ex.Message);
            }

            if (fileMetadatas.Count == 0) return;

            await Task.Run(() =>
            {
                try
                {
                    // Artists
                    List<string> distinctArtists = fileMetadatas.Select((f) => f.Artists.Value).Distinct().ToList();
                    this.Artists = new MetadataValue(distinctArtists.Count == 1 ? distinctArtists.First() : this.multipleValuesText);

                    // Title
                    List<string> distinctTitles = fileMetadatas.Select((f) => f.Title.Value).Distinct().ToList();
                    this.Title = new MetadataValue(distinctTitles.Count == 1 ? distinctTitles.First() : this.multipleValuesText);

                    // Album
                    List<string> distinctAlbums = fileMetadatas.Select((f) => f.Album.Value).Distinct().ToList();
                    this.Album = new MetadataValue(distinctAlbums.Count == 1 ? distinctAlbums.First() : this.multipleValuesText);

                    // AlbumArtists
                    List<string> distinctAlbumArtists = fileMetadatas.Select((f) => f.AlbumArtists.Value).Distinct().ToList();
                    this.AlbumArtists = new MetadataValue(distinctAlbumArtists.Count == 1 ? distinctAlbumArtists.First() : this.multipleValuesText);

                    // Year
                    List<string> distinctYears = fileMetadatas.Select((f) => f.Year.Value).Distinct().ToList();
                    this.Year = new MetadataValue(distinctYears.Count == 1 ? distinctYears.First().ToString() : this.multipleValuesText);

                    // TrackNumber
                    List<string> distinctTrackNumbers = fileMetadatas.Select((f) => f.TrackNumber.Value).Distinct().ToList();
                    this.TrackNumber = new MetadataValue(distinctTrackNumbers.Count == 1 ? distinctTrackNumbers.First().ToString() : this.multipleValuesText);

                    // TrackCount
                    List<string> distinctTrackCounts = fileMetadatas.Select((f) => f.TrackCount.Value).Distinct().ToList();
                    this.TrackCount = new MetadataValue(distinctTrackCounts.Count == 1 ? distinctTrackCounts.First().ToString() : this.multipleValuesText);

                    // DiscNumber
                    List<string> distinctDiscNumbers = fileMetadatas.Select((f) => f.DiscNumber.Value).Distinct().ToList();
                    this.DiscNumber = new MetadataValue(distinctDiscNumbers.Count == 1 ? distinctDiscNumbers.First().ToString() : this.multipleValuesText);

                    // DiscCount
                    List<string> distinctDiscCounts = fileMetadatas.Select((f) => f.DiscCount.Value).Distinct().ToList();
                    this.DiscCount = new MetadataValue(distinctDiscCounts.Count == 1 ? distinctDiscCounts.First().ToString() : this.multipleValuesText);

                    // Genres
                    List<string> distinctGenres = fileMetadatas.Select((f) => f.Genres.Value).Distinct().ToList();
                    this.Genres = new MetadataValue(distinctGenres.Count == 1 ? distinctGenres.First() : this.multipleValuesText);

                    // Grouping
                    List<string> distinctGroupings = fileMetadatas.Select((f) => f.Grouping.Value).Distinct().ToList();
                    this.Grouping = new MetadataValue(distinctGroupings.Count == 1 ? distinctGroupings.First() : this.multipleValuesText);

                    // Comment
                    List<string> distinctComments = fileMetadatas.Select((f) => f.Comment.Value).Distinct().ToList();
                    this.Comment = new MetadataValue(distinctComments.Count == 1 ? distinctComments.First() : this.multipleValuesText);

                    // Lyrics
                    List<string> distinctLyrics = fileMetadatas.Select((f) => f.Lyrics.Value).Distinct().ToList();
                    this.lyrics = new MetadataValue(distinctLyrics.Count == 1 ? distinctLyrics.First() : this.multipleValuesText);

                    // Artwork 
                    this.GetArtwork(fileMetadatas);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occured while parsing the metadata. Exception: {0}", ex.Message);
                }
            });
        }

        private void GetArtwork(List<FileMetadata> fileMetadatas)
        {
            byte[] foundArtwork = null;

            List<byte[]> artworks = fileMetadatas.Select((f) => f.ArtworkData.Value).ToList();
            List<int> artworksSizes = new List<int>();

            foreach (byte[] eaw in artworks)
            {
                if (eaw != null)
                {
                    artworksSizes.Add(eaw.Length);
                    foundArtwork = eaw;
                }
                else
                {
                    artworksSizes.Add(0);
                }
            }

            int distinctArtworkCount = artworksSizes.Select((l) => l).Distinct().Count();

            if (distinctArtworkCount > 1)
            {
                foundArtwork = null;
                this.HasMultipleArtwork = true;
            }
            else
            {
                this.HasMultipleArtwork = false;
            }

            this.VisualizeArtwork(foundArtwork);
        }

        private bool AllEntriesValid()
        {
            return this.Year.IsNumeric &
                   this.TrackNumber.IsNumeric &
                   this.TrackCount.IsNumeric &
                   this.DiscNumber.IsNumeric &
                   this.DiscCount.IsNumeric;
        }

        private void VisualizeArtwork(byte[] imageData)
        {
            // Size of the artwork
            if (imageData != null)
            {
                BitmapImage bi = ImageOperations.ByteToBitmapImage(imageData, 0, 0);

                // Use bi.PixelWidth and bi.PixelHeight instead of bi.Width and bi.Height:
                // bi.Width and bi.Height take DPI into account. We don't want that here.
                this.ArtworkSize = bi.PixelWidth + "x" + bi.PixelHeight;
            }
            else
            {
                this.ArtworkSize = string.Empty;
            }

            // Artwork data
            this.Artwork.Value = imageData;

            this.ArtworkThumbnail = ImageOperations.ByteToBitmapImage(imageData, Convert.ToInt32(Constants.CoverLargeSize), Convert.ToInt32(Constants.CoverLargeSize));
            OnPropertyChanged(() => this.HasArtwork);
        }

        private void UpdateArtwork(byte[] imageData)
        {
            // Set the artwork
            this.VisualizeArtwork(imageData);

            // Artwork is updated. Multiple artwork is now impossible.
            this.HasMultipleArtwork = false;
        }
        #endregion

        #region Public
        public async Task<bool> SaveTracksAsync()
        {
            if (!this.AllEntriesValid()) return false;

            List<FileMetadata> fmdList = new List<FileMetadata>();

            this.IsBusy = true;

            await Task.Run(() =>
            {
                try
                {
                    foreach (TrackInfo ti in this.trackInfos)
                    {
                        var fmd = new FileMetadata(ti.Path);

                        if (this.artists.IsValueChanged) fmd.Artists = this.artists;
                        if (this.title.IsValueChanged) fmd.Title = this.title;
                        if (this.album.IsValueChanged) fmd.Album = this.album;
                        if (this.albumArtists.IsValueChanged) fmd.AlbumArtists = this.albumArtists;
                        if (this.year.IsValueChanged) fmd.Year = this.year;
                        if (this.trackNumber.IsValueChanged) fmd.TrackNumber = this.trackNumber;
                        if (this.trackCount.IsValueChanged) fmd.TrackCount = this.trackCount;
                        if (this.discNumber.IsValueChanged) fmd.DiscNumber = this.discNumber;
                        if (this.discCount.IsValueChanged) fmd.DiscCount = this.discCount;
                        if (this.genres.IsValueChanged) fmd.Genres = this.genres;
                        if (this.grouping.IsValueChanged) fmd.Grouping = this.grouping;
                        if (this.comment.IsValueChanged) fmd.Comment = this.comment;
                        if (this.lyrics.IsValueChanged) fmd.Lyrics = this.lyrics;
                        if (this.artwork.IsValueChanged) fmd.ArtworkData = this.artwork;

                        fmdList.Add(fmd);
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occured while setting the metadata. Exception: {0}", ex.Message);
                }

            });

            if (fmdList.Count > 0) await this.metadataService.UpdateTrackAsync(fmdList, this.UpdateAlbumArtwork);

            this.IsBusy = false;
            return true;
        }
        #endregion
    }
}
