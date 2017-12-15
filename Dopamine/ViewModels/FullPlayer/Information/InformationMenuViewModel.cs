using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels.FullPlayer.Information
{
    public class InformationMenuViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;
        private InformationPage previousPage;
        private InformationPage selectedPage;

        public DelegateCommand LoadedCommand { get; set; }

        public InformationPage SelectedPage
        {
            get { return this.selectedPage; }
            set
            {
                SetProperty<InformationPage>(ref this.selectedPage, value);
                this.NagivateToSelectedPage();
            }
        }

        public InformationMenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.LoadedCommand = new DelegateCommand(() => this.NagivateToSelectedPage());
        }

        private void NagivateToSelectedPage()
        {
            this.eventAggregator.GetEvent<IsInformationPageChanged>().Publish(
                   new Tuple<SlideDirection, InformationPage>(this.selectedPage >= this.previousPage ? SlideDirection.RightToLeft : SlideDirection.LeftToRight, this.selectedPage));
            previousPage = this.selectedPage;
        }
    }
}
