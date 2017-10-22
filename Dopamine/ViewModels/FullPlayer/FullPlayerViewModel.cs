using Dopamine.Common.Enums;
using Dopamine.Common.Services.Indexing;
using Prism.Mvvm;

namespace Dopamine.ViewModels.FullPlayer
{
    public class FullPlayerViewModel : BindableBase
    {
        private FullPlayerPage selectedFullPlayerPage;
        private IIndexingService indexingService;

        public FullPlayerPage SelectedFullPlayerPage
        {
            get { return selectedFullPlayerPage; }
            set {
                SetProperty<FullPlayerPage>(ref this.selectedFullPlayerPage, value);

                if(value != FullPlayerPage.Settings)
                {
                    this.indexingService.AutoCheckCollectionIfFoldersChangedAsync();
                }
            }
        }

        public FullPlayerViewModel(IIndexingService indexingService)
        {
            this.indexingService = indexingService;
        }
    }
}
