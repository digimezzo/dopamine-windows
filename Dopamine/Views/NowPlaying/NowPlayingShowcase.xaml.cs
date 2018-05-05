using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Dopamine.Views.NowPlaying
{
    public partial class NowPlayingShowcase : UserControl
    {
        public NowPlayingShowcase()
        {
            InitializeComponent();
        }

        private void ResizePlaybackInfo()
        {
            Storyboard resizeCoverArtStoryboard = null;

            if (this.ActualHeight < 250)
            {
                resizeCoverArtStoryboard = this.PlaybackInfoPanel.Resources["ResizeCoverArt250"] as Storyboard;
            }
            else
            {
                for (int index = 250; index <= 600; index += 50)
                {
                    if (this.ActualHeight >= 1.5 * index)
                    {
                        resizeCoverArtStoryboard = this.PlaybackInfoPanel.Resources[string.Format("ResizeCoverArt{0}", index)] as Storyboard;
                    }
                }
            }

            if (resizeCoverArtStoryboard != null)
            {
                resizeCoverArtStoryboard.Begin();
            }
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.ResizePlaybackInfo();
        }
    }
}
