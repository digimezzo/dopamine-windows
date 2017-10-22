using Dopamine.Common.Enums;
using Dopamine.Common.Services.Indexing;
using Prism.Mvvm;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsViewModel : BindableBase
    {
        private SettingsPage selectedSettingsPage;
        private IIndexingService indexingService;

        public SettingsPage SelectedSettingsPage
        {
            get { return selectedSettingsPage; }
            set {
                SetProperty<SettingsPage>(ref this.selectedSettingsPage, value);

                if(value != SettingsPage.Collection)
                {
                    this.indexingService.AutoCheckCollectionIfFoldersChangedAsync();
                }
            }
        }

        public SettingsViewModel(IIndexingService indexingService)
        {
            this.indexingService = indexingService;
        }
    }
}
