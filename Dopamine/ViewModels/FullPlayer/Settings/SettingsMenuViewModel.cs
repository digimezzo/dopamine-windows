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
        private SettingsPage previousSelectedSettingsPage;
        private SettingsPage selectedSettingsPage;

        public DelegateCommand LoadedCommand { get; set; }

        public SettingsPage SelectedSettingsPage
        {
            get { return this.selectedSettingsPage; }
            set
            {
                SetProperty<SettingsPage>(ref this.selectedSettingsPage, value);
                this.NagivateToSelectedPage();
            }
        }

        public SettingsMenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.LoadedCommand = new DelegateCommand(() =>
            {
                this.SelectedSettingsPage = SettingsPage.Collection;
                this.NagivateToSelectedPage();
            });
        }

        private void NagivateToSelectedPage()
        {
            this.eventAggregator.GetEvent<IsSettingsPageChanged>().Publish(
                   new Tuple<SlideDirection, SettingsPage>(this.selectedSettingsPage >= this.previousSelectedSettingsPage ? SlideDirection.RightToLeft : SlideDirection.LeftToRight, this.selectedSettingsPage));
            previousSelectedSettingsPage = this.selectedSettingsPage;
        }
    }
}
