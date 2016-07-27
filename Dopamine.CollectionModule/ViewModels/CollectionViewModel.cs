using Dopamine.Core.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionViewModel : BindableBase
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private int slideInFrom;
        private int previousIndex = 0;
        #endregion

        #region Properties
        public DelegateCommand<string> NavigateBetweenCollectionCommand;

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }
        #endregion

        #region Construction
        public CollectionViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            this.NavigateBetweenCollectionCommand = new DelegateCommand<string>(NavigateBetweenCollection);
            ApplicationCommands.NavigateBetweenCollectionCommand.RegisterCommand(this.NavigateBetweenCollectionCommand);
            this.SlideInFrom = 30;
        }
        #endregion

        #region Functions
        private void NavigateBetweenCollection(string indexString)
        {
            if (string.IsNullOrWhiteSpace(indexString))
                return;

            int index = 0;

            int.TryParse(indexString, out index);

            if (index == 0)
                return;

            this.SlideInFrom = index <= this.previousIndex ? -30 : 30;

            this.previousIndex = index;

            this.regionManager.RequestNavigate(RegionNames.CollectionContentRegion, this.GetPageForIndex(index));
        }

        private string GetPageForIndex(int iIndex)
        {
            string page = string.Empty;

            switch (iIndex)
            {
                case 1:
                    page = typeof(Views.CollectionArtists).FullName;
                    break;
                case 2:
                    page = typeof(Views.CollectionGenres).FullName;
                    break;
                case 3:
                    page = typeof(Views.CollectionAlbums).FullName;
                    break;
                case 4:
                    page = typeof(Views.CollectionTracks).FullName;
                    break;
                case 5:
                    page = typeof(Views.CollectionPlaylists).FullName;
                    break;
                case 6:
                    page = typeof(Views.CollectionCloud).FullName;
                    break;
                default:
                    page = typeof(Views.CollectionArtists).FullName;
                    break;
            }

            return page;
        }
        #endregion
    }
}
