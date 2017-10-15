using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.NowPlaying
{
    public partial class NowPlaying : UserControl
    {
        private Timer hideControlsTimer = new Timer();

        public bool CanShowControls
        {
            get { return Convert.ToBoolean(GetValue(CanShowControlsProperty)); }
            set { SetValue(CanShowControlsProperty, value); }
        }

        public static readonly DependencyProperty CanShowControlsProperty =
            DependencyProperty.Register(nameof(CanShowControls), typeof(bool), typeof(NowPlaying), new PropertyMetadata(null));

        public NowPlaying()
        {
            InitializeComponent();

            this.hideControlsTimer.Interval = 2000;
            this.hideControlsTimer.Elapsed += new ElapsedEventHandler(this.CleanupNowPlayingHandler);
            this.ShowControls();
        }

        private void ShowControls()
        {
            this.hideControlsTimer.Stop();
            this.CanShowControls = true;
            this.hideControlsTimer.Start();
        }

        public void CleanupNowPlayingHandler(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!this.BackButton.IsMouseOver)
                {
                    this.CanShowControls = false;
                }
            }));
        }

        private void NowPlaying_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ShowControls();
        }

        private void SpectrumAnalyzer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AlignSpectrumAnalyzer();
        }

        private void NowPlaying_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AlignSpectrumAnalyzer();
        }

        private void AlignSpectrumAnalyzer()
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
    }
}
