using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class NowPlayingPlaybackControls : UserControl
    {
        public NowPlayingSubPage SelectedNowPlayingSubPage
        {
            get { return (NowPlayingSubPage)GetValue(SelectedNowPlayingSubPageProperty); }
            set { SetValue(SelectedNowPlayingSubPageProperty, value); }
        }

        public static readonly DependencyProperty SelectedNowPlayingSubPageProperty =
            DependencyProperty.Register(
                nameof(SelectedNowPlayingSubPage), 
                typeof(NowPlayingSubPage), 
                typeof(NowPlayingPlaybackControls), 
                new PropertyMetadata(NowPlayingSubPage.ShowCase, new PropertyChangedCallback(SelectedNowPlayingSubPagePropertyChanged)));

        private static void SelectedNowPlayingSubPagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is NowPlayingPlaybackControls)
            {
                SettingsClient.Set<int>("FullPlayer", "SelectedNowPlayingSubPage", (int)((NowPlayingPlaybackControls)d).SelectedNowPlayingSubPage);
            }
        }

        public NowPlayingPlaybackControls()
        {
            InitializeComponent();

            if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
            {
                this.SelectedNowPlayingSubPage = (NowPlayingSubPage)SettingsClient.Get<int>("FullPlayer", "SelectedNowPlayingSubPage");
            }
            else
            {
                this.SelectedNowPlayingSubPage = NowPlayingSubPage.ShowCase;
            }
        }
    }
}
