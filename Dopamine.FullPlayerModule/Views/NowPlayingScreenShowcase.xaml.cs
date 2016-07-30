using Prism.Mvvm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Dopamine.FullPlayerModule.Views
{
    public partial class NowPlayingScreenShowcase : UserControl
    {
        #region Properties
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }
        #endregion

        #region Construction
        public NowPlayingScreenShowcase()
        {
            InitializeComponent();
        }
        #endregion

        #region Private
        private void ResizePlaybackInfo()
        {
            Storyboard resizeCoverArtStoryboard = null;

            for (int index = 250; index <= 600; index += 50)
            {
                if (this.ActualHeight >= 1.5 * index)
                {
                    resizeCoverArtStoryboard = this.PlaybackInfoPanel.Resources[ string.Format("ResizeCoverArt{0}", index)] as Storyboard;
                }
            }

            if (resizeCoverArtStoryboard != null) resizeCoverArtStoryboard.Begin();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ResizePlaybackInfo();
        }
        #endregion
    }
}
