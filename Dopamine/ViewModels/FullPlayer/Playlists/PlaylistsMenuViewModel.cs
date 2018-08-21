using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsMenuViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;
        private PlaylistsPage previousPage;
        private PlaylistsPage selectedPage;

        public DelegateCommand LoadedCommand { get; set; }

        public PlaylistsPage SelectedPage
        {
            get { return this.selectedPage; }
            set
            {
                SetProperty<PlaylistsPage>(ref this.selectedPage, value);
                SettingsClient.Set<int>("FullPlayer", "SelectedPlaylistsPage", (int)value);
                this.NagivateToSelectedPage();
            }
        }

        public PlaylistsMenuViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.LoadedCommand = new DelegateCommand(() =>
            {
                if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
                {
                    this.SelectedPage = (PlaylistsPage)SettingsClient.Get<int>("FullPlayer", "SelectedPlaylistsPage");
                }
                else
                {
                    this.SelectedPage = PlaylistsPage.Playlists;
                }

                this.NagivateToSelectedPage();
            });
        }

        private void NagivateToSelectedPage()
        {
            this.eventAggregator.GetEvent<IsPlaylistsPageChanged>().Publish(
                   new Tuple<SlideDirection, PlaylistsPage>(this.selectedPage >= this.previousPage ? SlideDirection.RightToLeft : SlideDirection.LeftToRight, this.selectedPage));
            previousPage = this.selectedPage;
        }
    }
}
