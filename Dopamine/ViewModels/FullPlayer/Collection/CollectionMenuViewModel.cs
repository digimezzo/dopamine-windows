using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
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
                this.NagivateToSelectedPage();
            }
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
