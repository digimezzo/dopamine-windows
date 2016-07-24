using Dopamine.Core.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;

namespace Dopamine.InformationModule.ViewModels
{
    public class InformationViewModel : BindableBase
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private int previousIndex = 0;
        private int slideInFrom;
        #endregion

        #region Commands    
        public DelegateCommand<string> NavigateBetweenInformationCommand;
        #endregion

        #region Properties
        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }
        #endregion
    
        #region Construction
        public InformationViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.NavigateBetweenInformationCommand = new DelegateCommand<string>(NavigateBetweenInformation);
            ApplicationCommands.NavigateBetweenInformationCommand.RegisterCommand(this.NavigateBetweenInformationCommand);
            this.SlideInFrom = 30;
        }
        #endregion

        #region Private
        private void NavigateBetweenInformation(string iIndex)
        {
            if (string.IsNullOrWhiteSpace(iIndex))
                return;

            int index = 0;

            int.TryParse(iIndex, out index);

            if (index == 0)
                return;

            this.SlideInFrom = index <= this.previousIndex ? -30 : 30;

            this.previousIndex = index;

            this.regionManager.RequestNavigate(RegionNames.InformationRegion, this.GetPageForIndex(index));
        }

        private string GetPageForIndex(int iIndex)
        {

            string page = string.Empty;

            switch (iIndex)
            {
                case 1:
                    page = typeof(Views.InformationHelp).FullName;
                    break;
                case 2:
                    page = typeof(Views.InformationAbout).FullName;
                    break;
                default:
                    page = typeof(Views.InformationHelp).FullName;
                    break;
            }

            return page;
        }
        #endregion
    }
}
