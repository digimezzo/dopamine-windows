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
