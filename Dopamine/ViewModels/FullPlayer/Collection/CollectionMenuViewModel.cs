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
        private CollectionPage previousSelectedCollectionPage;
        private CollectionPage selectedCollectionPage;

        public DelegateCommand LoadedCommand { get; set; }

        public CollectionPage SelectedCollectionPage
        {
            get { return this.selectedCollectionPage; }
            set
            {
                SetProperty<CollectionPage>(ref this.selectedCollectionPage, value);
                SettingsClient.Set<int>("FullPlayer", "SelectedCollectionPage", (int)value);
                RaisePropertyChanged(nameof(this.CanSearch));
                this.NagivateToSelectedPage();
            }
        }

        public bool CanSearch
        {
            get { return this.selectedCollectionPage != CollectionPage.Frequent; }
        }

        public CollectionMenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.LoadedCommand = new DelegateCommand(() =>
            {
                if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
                {
                    this.SelectedCollectionPage = (CollectionPage)SettingsClient.Get<int>("FullPlayer", "SelectedCollectionPage");
                }
                else
                {
                    this.SelectedCollectionPage = CollectionPage.Artists;
                }

                this.NagivateToSelectedPage();
            });
        }

        private void NagivateToSelectedPage()
        {
            this.eventAggregator.GetEvent<IsCollectionPageChanged>().Publish(
                   new Tuple<SlideDirection, CollectionPage>(this.selectedCollectionPage >= this.previousSelectedCollectionPage ? SlideDirection.RightToLeft : SlideDirection.LeftToRight, this.selectedCollectionPage));
            previousSelectedCollectionPage = this.selectedCollectionPage;
        }
    }
}
