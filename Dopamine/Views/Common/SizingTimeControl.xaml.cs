using Dopamine.Services.Playback;
using Dopamine.Services.Playback;
using CommonServiceLocator;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class SizingTimeControl : UserControl
    {
        private IPlaybackService playbackService;
        private bool isVisible;

        public HorizontalAlignment TimeHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(TimeHorizontalAlignmentProperty); }
            set { SetValue(TimeHorizontalAlignmentProperty, value); }
        }
     
        public static readonly DependencyProperty TimeHorizontalAlignmentProperty = DependencyProperty.Register("TimeHorizontalAlignment", typeof(HorizontalAlignment), typeof(SizingTimeControl), new PropertyMetadata(HorizontalAlignment.Left));
    
        public SizingTimeControl()
        {
            InitializeComponent();

            this.playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            this.playbackService.PlaybackProgressChanged += (_, __) => this.SetTotalTimeVisibility();
            this.playbackService.PlaybackSuccess += (_,__) => this.SetTotalTimeVisibility();
            this.playbackService.PlaybackStopped += (_, __) => this.SetTotalTimeVisibility();

            // Default is collapsed
            this.TotalTime.Visibility = Visibility.Collapsed;
            this.isVisible = false;
        }
   
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
      
        private void TimeOuterBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetTotalTimeVisibility();
        }

        private void TimeInnerBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetTotalTimeVisibility();
        }
    }
}
