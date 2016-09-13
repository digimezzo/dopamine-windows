using Prism.Mvvm;
using System.Windows.Controls;
using System.Windows;

namespace Dopamine.ControlsModule.Views
{
    public partial class CollectionPlaybackControls : UserControl
    {
        #region Construction
        public CollectionPlaybackControls()
        {
            InitializeComponent();
        }
        #endregion

        private void PlaybackInfoPanel_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.SetPlaybackInfoPanelVisibility();
        }

        private void SetPlaybackInfoPanelVisibility()
        {
            if (this.PlaybackInfoPanel.ActualWidth >= 250)
            {
                this.CoverArtControl.Visibility = Visibility.Visible;
                this.CoverArtControl.Margin = new Thickness(0, 0, 10, 0);
            }
            else
            {
                // Using Visibility.Colapsed is not an option. It causes CoverArtControl
                // to remain invisible after calling Visibility.Visible.
                this.CoverArtControl.Visibility = Visibility.Hidden;
                this.CoverArtControl.Margin = new Thickness(-this.CoverArtControl.ActualWidth - 2, 0, 2, 0);
            }
        }

        private void SetTotalTimeVisibility()
        {
            if (this.TimeBorder.ActualWidth >= 90)
            {
                this.TotalTime.Visibility = Visibility.Visible;
            }
            else
            {
                this.TotalTime.Visibility = Visibility.Collapsed;
            }
        }

        private void TimeBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetTotalTimeVisibility();
        }
    }
}
