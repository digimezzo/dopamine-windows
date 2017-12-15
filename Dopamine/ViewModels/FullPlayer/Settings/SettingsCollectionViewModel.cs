using Digimezzo.Utilities.Settings;
using Dopamine.Data.Repositories.Interfaces;
using Dopamine.Services.Collection;
using Dopamine.Services.Indexing;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsCollectionViewModel : BindableBase
    {
        private bool isActive;
        private bool checkBoxIgnoreRemovedFilesChecked;
        private bool checkBoxRefreshCollectionAutomaticallyChecked;
        private bool checkBoxDownloadMissingAlbumCoversChecked;
        private IIndexingService indexingService;
        private ICollectionService collectionService;
        private ITrackRepository trackRepository;

        public DelegateCommand RefreshNowCommand { get; set; }
        public DelegateCommand ReloadAllCoversCommand { get; set; }
        public DelegateCommand ReloadMissingCoversCommand { get; set; }

        public bool IsActive
        {
            get { return this.isActive; }
            set { SetProperty<bool>(ref this.isActive, value); }
        }

        public bool CheckBoxIgnoreRemovedFilesChecked
        {
            get { return this.checkBoxIgnoreRemovedFilesChecked; }
            set
            {
                SettingsClient.Set<bool>("Indexing", "IgnoreRemovedFiles", value);
                SetProperty<bool>(ref this.checkBoxIgnoreRemovedFilesChecked, value);

                if (!value)
                {
                    this.trackRepository.ClearRemovedTrackAsync(); // Fire and forget
                }
            }
        }

        public bool CheckBoxDownloadMissingAlbumCoversChecked
        {
            get { return this.checkBoxDownloadMissingAlbumCoversChecked; }
            set
            {
                SettingsClient.Set<bool>("Covers", "DownloadMissingAlbumCovers", value);
                SetProperty<bool>(ref this.checkBoxDownloadMissingAlbumCoversChecked, value);

                if (value)
                {
                    this.indexingService.ReloadAlbumArtworkAsync(true);
                }
            }
        }

        public bool CheckBoxRefreshCollectionAutomaticallyChecked
        {
            get { return this.checkBoxRefreshCollectionAutomaticallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Indexing", "RefreshCollectionAutomatically", value, true);
                SetProperty<bool>(ref this.checkBoxRefreshCollectionAutomaticallyChecked, value);
            }
        }

        public SettingsCollectionViewModel(IIndexingService indexingService, ICollectionService collectionService,
            ITrackRepository trackRepository)
        {
            this.indexingService = indexingService;
            this.collectionService = collectionService;
            this.trackRepository = trackRepository;

            this.RefreshNowCommand = new DelegateCommand(this.RefreshNow);
            this.ReloadAllCoversCommand = new DelegateCommand(() => this.indexingService.ReloadAlbumArtworkAsync(false));
            this.ReloadMissingCoversCommand = new DelegateCommand(() => this.indexingService.ReloadAlbumArtworkAsync(true));

            this.GetCheckBoxesAsync();
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxRefreshCollectionAutomaticallyChecked = SettingsClient.Get<bool>("Indexing", "RefreshCollectionAutomatically");
                this.checkBoxIgnoreRemovedFilesChecked = SettingsClient.Get<bool>("Indexing", "IgnoreRemovedFiles");
                this.checkBoxDownloadMissingAlbumCoversChecked = SettingsClient.Get<bool>("Covers", "DownloadMissingAlbumCovers");
            });
        }

        private void RefreshNow()
        {
            this.indexingService.RefreshCollectionImmediatelyAsync();
        }
    }
}
