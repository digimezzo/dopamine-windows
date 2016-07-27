using Dopamine.Core.Settings;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionTracksColumnsViewModel : BindableBase
    {
        #region Private
        private bool checkBoxRatingVisible;
        private bool checkBoxRatingChecked;
        private bool checkBoxArtistChecked;
        private bool checkBoxAlbumChecked;
        private bool checkBoxGenreChecked;
        private bool checkBoxLengthChecked;
        private bool checkBoxAlbumArtistChecked;
        private bool checkBoxTrackNumberChecked;
        private bool checkBoxYearChecked;
        #endregion

        #region Properties
        public bool CheckBoxRatingVisible
        {
            get { return checkBoxRatingVisible; }
            set { SetProperty<bool>(ref this.checkBoxRatingVisible, value); }
        }

        public bool CheckBoxRatingChecked
        {
            get { return checkBoxRatingChecked; }
            set { SetProperty<bool>(ref this.checkBoxRatingChecked, value); }
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
        #endregion

        #region Commands
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        #endregion

        #region Construction
        public CollectionTracksColumnsViewModel()
        {
            if (XmlSettingsClient.Instance.Get<bool>("Behaviour", "EnableRating"))
            {
                this.CheckBoxRatingVisible = true;
            }
            else
            {
                this.CheckBoxRatingVisible = false;
            }

            this.GetVisibleColumns();
        }
        #endregion

        #region Private
        private void GetVisibleColumns()
        {
            Utils.GetVisibleSongsColumns(
                ref this.checkBoxRatingChecked, 
                ref this.checkBoxArtistChecked,
                ref this.checkBoxAlbumChecked,
                ref this.checkBoxGenreChecked,
                ref this.checkBoxLengthChecked,
                ref this.checkBoxAlbumArtistChecked,
                ref this.checkBoxTrackNumberChecked,
                ref this.checkBoxYearChecked);

            OnPropertyChanged(() => this.CheckBoxRatingChecked);
            OnPropertyChanged(() => this.CheckBoxArtistChecked);
            OnPropertyChanged(() => this.CheckBoxAlbumChecked);
            OnPropertyChanged(() => this.CheckBoxGenreChecked);
            OnPropertyChanged(() => this.CheckBoxLengthChecked);
            OnPropertyChanged(() => this.CheckBoxAlbumArtistChecked);
            OnPropertyChanged(() => this.CheckBoxTrackNumberChecked);
            OnPropertyChanged(() => this.CheckBoxYearChecked);
        }
        #endregion

        #region Public
        public async Task<bool> SetVisibleColumns()
        {
            await Task.Run(() =>
            {
                Utils.SetVisibleSongsColumns(
                    this.CheckBoxRatingChecked,
                    this.CheckBoxArtistChecked,
                    this.CheckBoxAlbumChecked,
                    this.CheckBoxGenreChecked,
                    this.CheckBoxLengthChecked,
                    this.CheckBoxAlbumArtistChecked,
                    this.CheckBoxTrackNumberChecked,
                    this.CheckBoxYearChecked);
            });

            return true;
        }
        #endregion
    }
}
