using Digimezzo.Foundation.Core.Settings;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionTracksColumnsViewModel : BindableBase
    {
        private bool checkBoxRatingVisible;
        private bool checkBoxLoveVisible;
        private bool checkBoxRatingChecked;
        private bool checkBoxLoveChecked;
        private bool checkBoxLyricsChecked;
        private bool checkBoxArtistChecked;
        private bool checkBoxAlbumChecked;
        private bool checkBoxGenreChecked;
        private bool checkBoxLengthChecked;
        private bool checkBoxPlayCountChecked;
        private bool checkBoxSkipCountChecked;
        private bool checkBoxDateLastPlayedChecked;
        private bool checkBoxDateAddedChecked;
        private bool checkBoxDateCreatedChecked;
        private bool checkBoxAlbumArtistChecked;
        private bool checkBoxTrackNumberChecked;
        private bool checkBoxYearChecked;
        private bool checkBoxBitrateChecked;

        public bool CheckBoxRatingVisible
        {
            get { return checkBoxRatingVisible; }
            set { SetProperty<bool>(ref this.checkBoxRatingVisible, value); }
        }

        public bool CheckBoxLoveVisible
        {
            get { return checkBoxLoveVisible; }
            set { SetProperty<bool>(ref this.checkBoxLoveVisible, value); }
        }

        public bool CheckBoxRatingChecked
        {
            get { return checkBoxRatingChecked; }
            set { SetProperty<bool>(ref this.checkBoxRatingChecked, value); }
        }

        public bool CheckBoxLoveChecked
        {
            get { return checkBoxLoveChecked; }
            set { SetProperty<bool>(ref this.checkBoxLoveChecked, value); }
        }

        public bool CheckBoxLyricsChecked
        {
            get { return checkBoxLyricsChecked; }
            set { SetProperty<bool>(ref this.checkBoxLyricsChecked, value); }
        }

        public bool CheckBoxArtistChecked
        {
            get { return checkBoxArtistChecked; }
            set { SetProperty<bool>(ref this.checkBoxArtistChecked, value); }
        }

        public bool CheckBoxAlbumChecked
        {
            get { return checkBoxAlbumChecked; }
            set { SetProperty<bool>(ref this.checkBoxAlbumChecked, value); }
        }

        public bool CheckBoxGenreChecked
        {
            get { return checkBoxGenreChecked; }
            set { SetProperty<bool>(ref this.checkBoxGenreChecked, value); }
        }

        public bool CheckBoxLengthChecked
        {
            get { return checkBoxLengthChecked; }
            set { SetProperty<bool>(ref this.checkBoxLengthChecked, value); }
        }

        public bool CheckBoxPlayCountChecked
        {
            get { return checkBoxPlayCountChecked; }
            set { SetProperty<bool>(ref this.checkBoxPlayCountChecked, value); }
        }

        public bool CheckBoxSkipCountChecked
        {
            get { return checkBoxSkipCountChecked; }
            set { SetProperty<bool>(ref this.checkBoxSkipCountChecked, value); }
        }

        public bool CheckBoxDateLastPlayedChecked
        {
            get { return checkBoxDateLastPlayedChecked; }
            set { SetProperty<bool>(ref this.checkBoxDateLastPlayedChecked, value); }
        }

        public bool CheckBoxDateAddedChecked
        {
            get { return checkBoxDateAddedChecked; }
            set { SetProperty<bool>(ref this.checkBoxDateAddedChecked, value); }
        }

        public bool CheckBoxDateCreatedChecked
        {
            get { return checkBoxDateCreatedChecked; }
            set { SetProperty<bool>(ref this.checkBoxDateCreatedChecked, value); }
        }

        public bool CheckBoxAlbumArtistChecked
        {
            get { return checkBoxAlbumArtistChecked; }
            set { SetProperty<bool>(ref this.checkBoxAlbumArtistChecked, value); }
        }

        public bool CheckBoxTrackNumberChecked
        {
            get { return checkBoxTrackNumberChecked; }
            set { SetProperty<bool>(ref this.checkBoxTrackNumberChecked, value); }
        }

        public bool CheckBoxYearChecked
        {
            get { return checkBoxYearChecked; }
            set { SetProperty<bool>(ref this.checkBoxYearChecked, value); }
        }

        public bool CheckBoxBitrateChecked
        {
            get { return checkBoxBitrateChecked; }
            set { SetProperty<bool>(ref this.checkBoxBitrateChecked, value); }
        }

        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        public CollectionTracksColumnsViewModel()
        {
            this.CheckBoxRatingVisible = SettingsClient.Get<bool>("Behaviour", "EnableRating");
            this.CheckBoxLoveVisible = SettingsClient.Get<bool>("Behaviour", "EnableLove");

            this.GetVisibleColumns();
        }

        private void GetVisibleColumns()
        {
            CollectionUtils.GetVisibleSongsColumns(
                ref this.checkBoxRatingChecked,
                ref this.checkBoxLoveChecked,
                ref this.checkBoxLyricsChecked,
                ref this.checkBoxArtistChecked,
                ref this.checkBoxAlbumChecked,
                ref this.checkBoxGenreChecked,
                ref this.checkBoxLengthChecked,
                ref this.checkBoxPlayCountChecked,
                ref this.checkBoxSkipCountChecked,
                ref this.checkBoxDateLastPlayedChecked,
                ref this.checkBoxDateAddedChecked,
                ref this.checkBoxDateCreatedChecked,
                ref this.checkBoxAlbumArtistChecked,
                ref this.checkBoxTrackNumberChecked,
                ref this.checkBoxYearChecked,
                ref this.checkBoxBitrateChecked);

            RaisePropertyChanged(nameof(this.CheckBoxRatingChecked));
            RaisePropertyChanged(nameof(this.CheckBoxLoveChecked));
            RaisePropertyChanged(nameof(this.CheckBoxLyricsChecked));
            RaisePropertyChanged(nameof(this.CheckBoxArtistChecked));
            RaisePropertyChanged(nameof(this.CheckBoxAlbumChecked));
            RaisePropertyChanged(nameof(this.CheckBoxGenreChecked));
            RaisePropertyChanged(nameof(this.CheckBoxLengthChecked));
            RaisePropertyChanged(nameof(this.CheckBoxPlayCountChecked));
            RaisePropertyChanged(nameof(this.CheckBoxSkipCountChecked));
            RaisePropertyChanged(nameof(this.CheckBoxDateLastPlayedChecked));
            RaisePropertyChanged(nameof(this.CheckBoxDateAddedChecked));
            RaisePropertyChanged(nameof(this.CheckBoxDateCreatedChecked));
            RaisePropertyChanged(nameof(this.CheckBoxAlbumArtistChecked));
            RaisePropertyChanged(nameof(this.CheckBoxTrackNumberChecked));
            RaisePropertyChanged(nameof(this.CheckBoxYearChecked));
            RaisePropertyChanged(nameof(this.CheckBoxBitrateChecked));
        }

        public async Task<bool> SetVisibleColumns()
        {
            await Task.Run(() =>
            {
                CollectionUtils.SetVisibleSongsColumns(
                    this.CheckBoxRatingChecked,
                    this.CheckBoxLoveChecked,
                    this.CheckBoxLyricsChecked,
                    this.CheckBoxArtistChecked,
                    this.CheckBoxAlbumChecked,
                    this.CheckBoxGenreChecked,
                    this.CheckBoxLengthChecked,
                    this.CheckBoxPlayCountChecked,
                    this.CheckBoxSkipCountChecked,
                    this.CheckBoxDateLastPlayedChecked,
                    this.CheckBoxDateAddedChecked,
                    this.CheckBoxDateCreatedChecked,
                    this.CheckBoxAlbumArtistChecked,
                    this.CheckBoxTrackNumberChecked,
                    this.CheckBoxYearChecked,
                    this.checkBoxBitrateChecked);
            });

            return true;
        }
    }
}
