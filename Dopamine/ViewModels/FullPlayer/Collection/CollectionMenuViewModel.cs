using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionMenuViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;
        private CollectionPage previousPage;
        private CollectionPage selectedPage;

        public DelegateCommand LoadedCommand { get; set; }

        public CollectionPage SelectedPage
        {
            get { return this.selectedPage; }
            set
            {
                SetProperty<CollectionPage>(ref this.selectedPage, value);
                SettingsClient.Set<int>("FullPlayer", "SelectedCollectionPage", (int)value);
                RaisePropertyChanged(nameof(this.CanSearch));
                this.NagivateToSelectedPage();
            }
        }

        public bool CanSearch
        {
            get { return this.selectedPage != CollectionPage.Frequent; }
        }

        public CollectionMenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.LoadedCommand = new DelegateCommand(() =>
            {
                if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
                {
                    this.SelectedPage = (CollectionPage)SettingsClient.Get<int>("FullPlayer", "SelectedCollectionPage");
                }
                else
                {
                    this.SelectedPage = CollectionPage.Artists;
                }

                this.NagivateToSelectedPage();
            });
        }

        private void NagivateToSelectedPage()
        {
            this.eventAggregator.GetEvent<IsCollectionPageChanged>().Publish(
                   new Tuple<SlideDirection, CollectionPage>(this.selectedPage >= this.previousPage ? SlideDirection.RightToLeft : SlideDirection.LeftToRight, this.selectedPage));
            previousPage = this.selectedPage;
        }
    }
}
