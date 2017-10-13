using Digimezzo.Utilities.Settings;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Indexing;
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
        private IIndexingService indexingService;
        private ICollectionService collectionService;
        private ITrackRepository trackRepository;

        public DelegateCommand RefreshNowCommand { get; set; }

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
                if (!value) this.trackRepository.ClearRemovedTrackAsync(); // Fire and forget.
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

            this.GetCheckBoxesAsync();
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxRefreshCollectionAutomaticallyChecked = SettingsClient.Get<bool>("Indexing", "RefreshCollectionAutomatically");

                // Set the backing field of the property. This avoids executing a clear
                // of removed tracks when loading the screen when the setting is false.
                this.checkBoxIgnoreRemovedFilesChecked = SettingsClient.Get<bool>("Indexing", "IgnoreRemovedFiles");
                RaisePropertyChanged(nameof(this.CheckBoxIgnoreRemovedFilesChecked));
            });
        }

        private void RefreshNow()
        {
            this.indexingService.QuickCheckCollectionAsync();
        }
    }
}
