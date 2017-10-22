using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Prism.Mvvm;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionViewModel : BindableBase
    {
        private CollectionPage selectedCollectionPage;

        public CollectionPage SelectedCollectionPage
        {
            get { return this.selectedCollectionPage; }
            set
            {
                SetProperty<CollectionPage>(ref this.selectedCollectionPage, value);
                SettingsClient.Set<int>("FullPlayer", "SelectedCollectionPage", (int)value);
                RaisePropertyChanged(nameof(this.CanSearch));
            }
        }

        public bool CanSearch
        {
            get { return this.selectedCollectionPage != CollectionPage.Frequent; }
        }


        public CollectionViewModel()
        {
            if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
            {
                this.SelectedCollectionPage = (CollectionPage)SettingsClient.Get<int>("FullPlayer", "SelectedCollectionPage");
            }
            else
            {
                this.SelectedCollectionPage = CollectionPage.Artists;
            }
        }
    }
}
