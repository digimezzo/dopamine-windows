using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
using Dopamine.Data.Repositories;
using Dopamine.Services.Indexing;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer
{
    public class FullPlayerAddMusicViewModel : BindableBase
    {
        private bool checkBoxRefreshCollectionAutomaticallyChecked;
        private bool checkBoxIgnoreRemovedFilesChecked;
        private bool checkBoxDownloadMissingAlbumCoversChecked;
        private ITrackRepository trackRepository;
        private IIndexingService indexingService;

        public FullPlayerAddMusicViewModel(ITrackRepository trackRepository, IIndexingService indexingService)
        {
            this.trackRepository = trackRepository;
            this.indexingService = indexingService;
            this.RefreshNowCommand = new DelegateCommand(() => this.indexingService.RefreshCollectionImmediatelyAsync());
            this.ReloadAllCoversCommand = new DelegateCommand(() => this.indexingService.ReScanAlbumArtworkAsync(false));
            this.ReloadMissingCoversCommand = new DelegateCommand(() => this.indexingService.ReScanAlbumArtworkAsync(true));
            this.GetCheckBoxesAsync();
        }

        public DelegateCommand RefreshNowCommand { get; set; }

        public DelegateCommand ReloadAllCoversCommand { get; set; }

        public DelegateCommand ReloadMissingCoversCommand { get; set; }

        public bool CheckBoxRefreshCollectionAutomaticallyChecked
        {
            get { return this.checkBoxRefreshCollectionAutomaticallyChecked; }
            set
            {
                SettingsClient.Set<bool>("Indexing", "RefreshCollectionAutomatically", value, true);
                SetProperty<bool>(ref this.checkBoxRefreshCollectionAutomaticallyChecked, value);
            }
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
                    // Fire and forget
                    this.trackRepository.ClearRemovedTrackAsync(); 
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
                    this.indexingService.ReScanAlbumArtworkAsync(true);
                }
            }
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
    }
}
