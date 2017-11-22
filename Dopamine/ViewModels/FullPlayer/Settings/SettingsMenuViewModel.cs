using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsMenuViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;
        private SettingsPage previousPage;
        private SettingsPage selectedPage;

        public DelegateCommand LoadedCommand { get; set; }

        public SettingsPage SelectedPage
        {
            get { return this.selectedPage; }
            set
            {
                SetProperty<SettingsPage>(ref this.selectedPage, value);
                this.NagivateToSelectedPage();
            }
        }

        public SettingsMenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage());
        }

        private void NagivateToSelectedPage()
        {
            this.eventAggregator.GetEvent<IsSettingsPageChanged>().Publish(
                   new Tuple<SlideDirection, SettingsPage>(this.selectedPage >= this.previousPage ? SlideDirection.RightToLeft : SlideDirection.LeftToRight, this.selectedPage));
            previousPage = this.selectedPage;
        }
    }
}
