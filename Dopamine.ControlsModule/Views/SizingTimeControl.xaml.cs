using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ServiceLocation;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.ControlsModule.Views
{
    /// <summary>
    /// Interaction logic for SizingTimeControl.xaml
    /// </summary>
    public partial class SizingTimeControl : UserControl
    {
        #region Variables
        private IPlaybackService playbackService;
        private bool isVisible;
        #endregion

         #region Properties
        public HorizontalAlignment TimeHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(TimeHorizontalAlignmentProperty); }
            set { SetValue(TimeHorizontalAlignmentProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty TimeHorizontalAlignmentProperty = DependencyProperty.Register("TimeHorizontalAlignment", typeof(HorizontalAlignment), typeof(SizingTimeControl), new PropertyMetadata(HorizontalAlignment.Left));
        #endregion

        #region Construction
        public SizingTimeControl()
        {
            InitializeComponent();

            this.playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            this.playbackService.PlaybackProgressChanged += (_, __) => this.SetTotalTimeVisibility();
            this.playbackService.PlaybackSuccess += (_) => this.SetTotalTimeVisibility();
            this.playbackService.PlaybackStopped += (_, __) => this.SetTotalTimeVisibility();

            // Default is collapsed
            this.TotalTime.Visibility = Visibility.Collapsed;
            this.isVisible = false;
        }
        #endregion

        #region Private
        private void SetTotalTimeVisibility()
        {
            if (this.TimeInnerBorder.ActualWidth >= this.TimeOuterBorder.ActualWidth)
            {
                if (this.isVisible) Application.Current.Dispatcher.Invoke(() => { this.TotalTime.Visibility = Visibility.Collapsed; });
                this.isVisible = false;
            }
            else
            {
                if (!this.isVisible) Application.Current.Dispatcher.Invoke(() => { this.TotalTime.Visibility = Visibility.Visible; });
                this.isVisible = true;
            }
        }
        #endregion

        #region Event handlers
        private void TimeOuterBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetTotalTimeVisibility();
        }

        private void TimeInnerBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetTotalTimeVisibility();
        }
        #endregion
    }
}
