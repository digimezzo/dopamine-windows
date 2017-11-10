using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels.FullPlayer.Information
{
    public class InformationMenuViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;
        private InformationPage previousSelectedInformationPage;
        private InformationPage selectedInformationPage;

        public DelegateCommand LoadedCommand { get; set; }

        public InformationPage SelectedInformationPage
        {
            get { return this.selectedInformationPage; }
            set
            {
                SetProperty<InformationPage>(ref this.selectedInformationPage, value);
                this.NagivateToSelectedPage();
            }
        }

        public InformationMenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.LoadedCommand = new DelegateCommand(() =>
            {
                this.SelectedInformationPage = InformationPage.Help;
                this.NagivateToSelectedPage();
            });
        }

        private void NagivateToSelectedPage()
        {
            this.eventAggregator.GetEvent<IsInformationPageChanged>().Publish(
                   new Tuple<SlideDirection, InformationPage>(this.selectedInformationPage >= this.previousSelectedInformationPage ? SlideDirection.RightToLeft : SlideDirection.LeftToRight, this.selectedInformationPage));
            previousSelectedInformationPage = this.selectedInformationPage;
        }
    }
}
