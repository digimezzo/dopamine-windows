using Dopamine.Common.Base;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public class NavigationViewModelBase: BindableBase
    {
        private IRegionManager regionManager;
        private int slideInFrom;

        public DelegateCommand LoadedCommand { get; set; }

        public IRegionManager RegionManager => this.regionManager;
        
        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public NavigationViewModelBase(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.SlideInFrom = Constants.SlideDistance;
        }
    }
}
