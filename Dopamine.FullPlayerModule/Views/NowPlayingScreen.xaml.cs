using Dopamine.Common.Base;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.FullPlayerModule.Views
{
    public partial class NowPlayingScreen : UserControl
    {
        #region Variables
        private Timer cleanupNowPlayingTimer = new Timer();
        private int cleanupNowPlayingTimeout = 2; // 2 seconds
        #endregion

        #region Dependency properties
        public static readonly DependencyProperty ShowControlsProperty = DependencyProperty.Register("ShowControls", typeof(bool), typeof(NowPlayingScreen), new PropertyMetadata(null));
        #endregion

        #region Properties
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }

        public bool ShowControls
        {
            get { return Convert.ToBoolean(GetValue(ShowControlsProperty)); }

            set { SetValue(ShowControlsProperty, value); }
        }
        #endregion

        #region Construction
        public NowPlayingScreen()
        {
            InitializeComponent();

            this.cleanupNowPlayingTimer.Interval = TimeSpan.FromSeconds(this.cleanupNowPlayingTimeout).TotalMilliseconds;
            this.cleanupNowPlayingTimer.Elapsed += new ElapsedEventHandler(this.CleanupNowPlayingHandler);
            this.SetNowPlaying(true);
        }
        #endregion

        #region Private
        private void SetNowPlaying(bool cluttered)
        {
            if (cluttered)
            {
                this.cleanupNowPlayingTimer.Stop();
                this.ShowControls = true;
                this.cleanupNowPlayingTimer.Start();
            }
            else
            {
                this.cleanupNowPlayingTimer.Stop();
                this.cleanupNowPlayingTimer.Start();
            }
        }
        #endregion

        #region Event handlers
        public void CleanupNowPlayingHandler(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!this.BackButton.IsMouseOver)
                {
                    this.ShowControls = false;
                }
            }));
        }

        private void NowPlaying_MouseMove(object sender, MouseEventArgs e)
        {
            SetNowPlaying(true);
        }

        private void NowPlaying_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // This makes sure the spectrum analyzer is centered on the screen, based on the left pixel.
            // When we align center, alignment is sometimes (depending on the width of the screen) done
            // on a half pixel. This causes a blurry spectrum analyzer.
            try
            {
                this.SpectrumAnalyzer.Margin = new Thickness(Convert.ToInt32(this.ActualWidth / 2) - Convert.ToInt32(this.SpectrumAnalyzer.ActualWidth / 2), 0, 0, 0);
            }
            catch (Exception)
            {
                // Swallow this exception
            }
        }

        private async void NowPlaying_Loaded(object sender, RoutedEventArgs e)
        {
            // Duration is set after 1/2 second so the now playing info screen doesn't 
            // slide in from the left the first time the now playing screen is loaded.
            // Slide in from left combined with slide in from bottom for the cover picture,
            // gives as combined effect a slide in from bottomleft for the cover picture.
            // That doesn't look so good.
            await Task.Delay(500);
            this.NowPlayingContentRegion.SlideDuration = Constants.SlideTimeoutSeconds;
            this.NowPlayingContentRegion.FadeInDuration = Constants.FadeInTimeoutSeconds;
            this.NowPlayingContentRegion.FadeOutDuration = Constants.FadeOutTimeoutSeconds;
        }
        #endregion
    }
}
