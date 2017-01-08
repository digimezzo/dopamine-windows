using Digimezzo.Utilities.Utils;
using Dopamine.CollectionModule.Views;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Database;
using Dopamine.Common.Prism;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System.Threading.Tasks;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionTracksViewModel : CommonTracksViewModel
    {
        #region Variables
        // Flags
        private bool ratingVisible;
        private bool loveVisible;
        private bool lyricsVisible;
        private bool artistVisible;
        private bool albumVisible;
        private bool genreVisible;
        private bool lengthVisible;
        private bool albumArtistVisible;
        private bool trackNumberVisible;
        private bool yearVisible;
        private bool bitrateVisible;
        #endregion

        #region Properties
        public bool RatingVisible
        {
            get { return this.ratingVisible; }
            set { SetProperty<bool>(ref this.ratingVisible, value); }
        }

        public bool LoveVisible
        {
            get { return this.loveVisible; }
            set { SetProperty<bool>(ref this.loveVisible, value); }
        }

        public bool LyricsVisible
        {
            get { return this.lyricsVisible; }
            set { SetProperty<bool>(ref this.lyricsVisible, value); }
        }

        public bool ArtistVisible
        {
            get { return this.artistVisible; }
            set { SetProperty<bool>(ref this.artistVisible, value); }
        }

        public bool AlbumVisible
        {
            get { return this.albumVisible; }
            set { SetProperty<bool>(ref this.albumVisible, value); }
        }

        public bool GenreVisible
        {
            get { return this.genreVisible; }
            set { SetProperty<bool>(ref this.genreVisible, value); }
        }

        public bool LengthVisible
        {
            get { return this.lengthVisible; }
            set { SetProperty<bool>(ref this.lengthVisible, value); }
        }

        public bool AlbumArtistVisible
        {
            get { return this.albumArtistVisible; }
            set { SetProperty<bool>(ref this.albumArtistVisible, value); }
        }

        public bool TrackNumberVisible
        {
            get { return this.trackNumberVisible; }
            set { SetProperty<bool>(ref this.trackNumberVisible, value); }
        }

        public bool YearVisible
        {
            get { return this.yearVisible; }
            set { SetProperty<bool>(ref this.yearVisible, value); }
        }

        public bool BitrateVisible
        {
            get { return this.bitrateVisible; }
            set { SetProperty<bool>(ref this.bitrateVisible, value); }
        }

        public override bool CanOrderByAlbum
        {
            // Doesn't need to return a useful value in this class
            get { return false; }
        }
        #endregion

        #region Commands
        public DelegateCommand ChooseColumnsCommand { get; set; }
        #endregion

        #region Construction
        public CollectionTracksViewModel(IUnityContainer container) : base(container)
        {
            // Commands
            this.ChooseColumnsCommand = new DelegateCommand(this.ChooseColumns);
            this.AddTracksToPlaylistCommand = new DelegateCommand<string>(async (playlistName) => await this.AddTracksToPlaylistAsync(this.SelectedTracks, playlistName));
            this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.RemoveTracksFromCollectionAsync(this.SelectedTracks), () => !this.IsIndexing);
            this.RemoveSelectedTracksFromDiskCommand = new DelegateCommand(async () => await this.RemoveTracksFromDiskAsync(this.SelectedTracks), () => !this.IsIndexing);

            // Events
            this.eventAggregator.GetEvent<RemoveSelectedTracksWithKeyDelete>().Subscribe((screenName) =>
            {
                if (screenName == typeof(CollectionTracks).FullName) this.RemoveSelectedTracksCommand.Execute();
            });

            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe((enableRating) =>
            {
                this.EnableRating = enableRating;
                this.GetVisibleColumns();
            });

            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe((enableLove) =>
            {
                this.EnableLove = enableLove;
                this.GetVisibleColumns();
            });

            // MetadataService
            this.metadataService.MetadataChanged += MetadataChangedHandlerAsync;

            // Show only the columns which are visible
            this.GetVisibleColumns();

            // Subscribe to Events and Commands on creation
            this.Subscribe();
        }
        #endregion

        #region Private
        private async void MetadataChangedHandlerAsync(MetadataChangedEventArgs e)
        {
            if (e.IsTrackChanged)
            {
                await this.GetTracksAsync(null, null, null, TrackOrder.ByAlbum);
            }
        }

        private void ChooseColumns()
        {
            CollectionTracksColumns view = this.container.Resolve<CollectionTracksColumns>();
            view.DataContext = this.container.Resolve<CollectionTracksColumnsViewModel>();

            this.dialogService.ShowCustomDialog(
                0xe73e,
                16,
                ResourceUtils.GetStringResource("Language_Columns"),
                view,
                400,
                0,
                false,
                true,
                true,
                true,
                ResourceUtils.GetStringResource("Language_Ok"),
                ResourceUtils.GetStringResource("Language_Cancel"),
                ((CollectionTracksColumnsViewModel)view.DataContext).SetVisibleColumns);

            // When the dialog is closed, update the columns
            this.GetVisibleColumns();
        }

        private void GetVisibleColumns()
        {
            bool columnRatingVisible = false;
            bool columnLoveVisible = false;

            Utils.GetVisibleSongsColumns(
                ref columnRatingVisible,
                ref columnLoveVisible,
                ref this.lyricsVisible,
                ref this.artistVisible,
                ref this.albumVisible,
                ref this.genreVisible,
                ref this.lengthVisible,
                ref this.albumArtistVisible,
                ref this.trackNumberVisible,
                ref this.yearVisible,
                ref this.bitrateVisible);

            OnPropertyChanged(() => this.LyricsVisible);
            OnPropertyChanged(() => this.ArtistVisible);
            OnPropertyChanged(() => this.AlbumVisible);
            OnPropertyChanged(() => this.GenreVisible);
            OnPropertyChanged(() => this.LengthVisible);
            OnPropertyChanged(() => this.AlbumArtistVisible);
            OnPropertyChanged(() => this.TrackNumberVisible);
            OnPropertyChanged(() => this.YearVisible);
            OnPropertyChanged(() => this.BitrateVisible);


            this.RatingVisible = this.EnableRating && columnRatingVisible;
            this.LoveVisible = this.EnableLove && columnLoveVisible;
        }
        #endregion

        #region Overrides
        protected async override Task FillListsAsync()
        {
            await this.GetTracksAsync(null, null, null, TrackOrder.ByAlbum);
        }

        protected override void Unsubscribe()
        {
            // Commands
            ApplicationCommands.AddTracksToPlaylistCommand.UnregisterCommand(this.AddTracksToPlaylistCommand);
        }

        protected override void Subscribe()
        {
            // Prevents subscribing twice
            this.Unsubscribe();

            // Commands
            ApplicationCommands.AddTracksToPlaylistCommand.RegisterCommand(this.AddTracksToPlaylistCommand);
        }

        protected override void RefreshLanguage()
        {
            // Do Nothing
        }
        #endregion
    }

}
