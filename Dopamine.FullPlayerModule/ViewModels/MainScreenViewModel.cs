using Dopamine.Common.Presentation.Views;
using Dopamine.ControlsModule.Views;
using Dopamine.Core.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class MainScreenViewModel : BindableBase
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private int previousIndex = 0;
        private int subMenuSlideInFrom;
        private int searchSlideInFrom;
        private int contentSlideInFrom;
        #endregion

        #region Commands
        public DelegateCommand<string> NavigateBetweenMainCommand { get; set; }
        #endregion

        #region Properties
        public int SubMenuSlideInFrom
        {
            get { return this.subMenuSlideInFrom; }
            set { SetProperty<int>(ref this.subMenuSlideInFrom, value); }
        }

        public int SearchSlideInFrom
        {
            get { return this.searchSlideInFrom; }
            set { SetProperty<int>(ref this.searchSlideInFrom, value); }
        }

        public int ContentSlideInFrom
        {
            get { return this.contentSlideInFrom; }
            set { SetProperty<int>(ref this.contentSlideInFrom, value); }
        }
        #endregion

        #region Construction
        public MainScreenViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            this.NavigateBetweenMainCommand = new DelegateCommand<string>((index) => this.NavigateBetweenMain(index));
            ApplicationCommands.NavigateBetweenMainCommand.RegisterCommand(this.NavigateBetweenMainCommand);
            this.SubMenuSlideInFrom = 30;
            this.SearchSlideInFrom = 30;
            this.ContentSlideInFrom = 30;
        }
        #endregion

        #region Private
        private void NavigateBetweenMain(string indexString)
        {
            if (string.IsNullOrWhiteSpace(indexString))
                return;

            int index = 0;

            int.TryParse(indexString, out index);

            if (index == 0)
                return;

            this.SubMenuSlideInFrom = index <= this.previousIndex ? 0 : 30;
            this.SearchSlideInFrom = index <= this.previousIndex ? -30 : 30;
            this.ContentSlideInFrom = index <= this.previousIndex ? -30 : 30;

            this.previousIndex = index;

            if (index == 2)
            {
                // Settings
                this.regionManager.RequestNavigate(RegionNames.ContentRegion, typeof(SettingsModule.Views.Settings).FullName);
                this.regionManager.RequestNavigate(RegionNames.SubMenuRegion, typeof(SettingsModule.Views.SettingsSubMenu).FullName);
                this.regionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, typeof(Empty).FullName);
            }
            else if (index == 3)
            {
                // Information
                this.regionManager.RequestNavigate(RegionNames.ContentRegion, typeof(InformationModule.Views.Information).FullName);
                this.regionManager.RequestNavigate(RegionNames.SubMenuRegion, typeof(InformationModule.Views.InformationSubMenu).FullName);
                this.regionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, typeof(Empty).FullName);
            }
            else
            {
                // Collection
                this.regionManager.RequestNavigate(RegionNames.ContentRegion, typeof(CollectionModule.Views.Collection).FullName);
                this.regionManager.RequestNavigate(RegionNames.SubMenuRegion, typeof(CollectionModule.Views.CollectionSubMenu).FullName);
                this.regionManager.RequestNavigate(RegionNames.FullPlayerSearchRegion, typeof(SearchControl).FullName);
            }
        }
        #endregion
    }
}
