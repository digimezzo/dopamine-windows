using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Base;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Dopamine.Views.FullPlayer.Information;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Information
{
    public class InformationViewModel : BindableBase
    {
        private int slideInFrom;
        private IRegionManager regionManager;

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public InformationViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            eventAggregator.GetEvent<IsInformationPageChanged>().Subscribe(tuple =>
            {
                this.NagivateToPage(tuple.Item1, tuple.Item2);
            });
        }

        private void NagivateToPage(SlideDirection direction, InformationPage page)
        {
            this.SlideInFrom = direction == SlideDirection.RightToLeft ? Constants.SlideDistance : -Constants.SlideDistance;

            switch (page)
            {
                case InformationPage.Help:
                    this.regionManager.RequestNavigate(RegionNames.InformationRegion, typeof(InformationHelp).FullName);
                    break;
                case InformationPage.About:
                    this.regionManager.RequestNavigate(RegionNames.InformationRegion, typeof(InformationAbout).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
