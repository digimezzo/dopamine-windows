using Dopamine.Core.Settings;
using Prism.Mvvm;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionSubMenuViewModel : BindableBase
    {
        #region Variables
        private bool isArtistsSelected;
        private bool isGenresSelected;
        private bool isAlbumsSelected;
        private bool isTracksSelected;
        private bool isPlaylistsSelected;
        private bool isCloudSelected;
        #endregion

        #region Properties
        public bool IsArtistsSelected
        {
            get { return this.isArtistsSelected; }
            set { SetProperty<bool>(ref this.isArtistsSelected, value); }
        }

        public bool IsGenresSelected
        {
            get { return this.isGenresSelected; }
            set { SetProperty<bool>(ref this.isGenresSelected, value); }
        }

        public bool IsAlbumsSelected
        {
            get { return this.isAlbumsSelected; }
            set { SetProperty<bool>(ref this.isAlbumsSelected, value); }
        }

        public bool IsTracksSelected
        {
            get { return this.isTracksSelected; }
            set { SetProperty<bool>(ref this.isTracksSelected, value); }
        }

        public bool IsPlaylistsSelected
        {
            get { return this.isPlaylistsSelected; }
            set { SetProperty<bool>(ref this.isPlaylistsSelected, value); }
        }

        public bool IsCloudSelected
        {
            get { return this.isCloudSelected; }
            set { SetProperty<bool>(ref this.isCloudSelected, value); }
        }
        #endregion

        #region Construction
        public CollectionSubMenuViewModel()
        {
            this.SelectMenuItem();
        }
        #endregion

        #region Private
        private void SelectMenuItem()
        {
            if (XmlSettingsClient.Instance.Get<bool>("Startup", "ShowLastSelectedPage"))
            {
                SelectedPage screen = (SelectedPage)XmlSettingsClient.Instance.Get<int>("FullPlayer", "SelectedPage");

                switch (screen)
                {
                    case SelectedPage.Artists:
                        this.IsArtistsSelected = true;
                        break;
                    case SelectedPage.Genres:
                        this.IsGenresSelected = true;
                        break;
                    case SelectedPage.Albums:
                        this.IsAlbumsSelected = true;
                        break;
                    case SelectedPage.Tracks:
                        this.IsTracksSelected = true;
                        break;
                    case SelectedPage.Playlists:
                        this.IsPlaylistsSelected = true;
                        break;
                    case SelectedPage.Recent:
                        this.IsCloudSelected = true;
                        break;
                    default:
                        this.IsArtistsSelected = true;
                        break;
                }
            }
            else
            {
                this.IsArtistsSelected = true;
            }
        }
        #endregion
    }
}
