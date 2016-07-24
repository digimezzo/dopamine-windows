using Dopamine.Common.Services.Playback;
using Dopamine.Core.Logging;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Common.Presentation.Views
{
    /// <summary>
    /// Interaction logic for PopupVolumeControls.xaml
    /// </summary>
    public partial class PopupVolumeControls : UserControl, IView
    {
        #region Variables
        private IPlaybackService playBackService;
        private Timer mouseWheelTimer;
        private double mouseWheelTimeout = 0.5;
        private bool keepOpenAfterScrolling;
        #endregion

        #region Properties
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }
        #endregion

        #region Construction
        public PopupVolumeControls()
        {
            InitializeComponent();

            // We need a parameterless constructor to be able to use this UserControl in other UserControls without dependency injection.
            // So for now there is no better solution than to find the EventAggregator by using the ServiceLocator.
            this.playBackService = ServiceLocator.Current.GetInstance<IPlaybackService>();

            this.mouseWheelTimer = new Timer();
            this.mouseWheelTimer.Interval = TimeSpan.FromSeconds(this.mouseWheelTimeout).TotalMilliseconds;
            this.mouseWheelTimer.Elapsed += new ElapsedEventHandler(this.MouseWheelTimerElapsed);
            this.VolumeButtonPopup.Closed += (sender,e) => this.keepOpenAfterScrolling = false;

            // This doesn't work with binding
            this.VolumeButton.Width = this.Width;
            this.VolumeButton.Height = this.Height;
        }
        #endregion

        #region Private
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            this.keepOpenAfterScrolling = true;
            this.mouseWheelTimer.Stop();
            VolumeButtonPopup.Open();
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!VolumeButtonPopup.IsOpen)
            {
                VolumeButtonPopup.Open();
            }

            this.mouseWheelTimer.Stop();

            if (!this.keepOpenAfterScrolling)
                this.mouseWheelTimer.Start();

            try
            {
                this.playBackService.Volume = Convert.ToSingle(this.playBackService.Volume + ((double)e.Delta / 5000));
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem changing the volume by mouse scroll. Exception: {0}", ex.Message);
            }
        }

        private void MouseWheelTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.mouseWheelTimer != null)
                this.mouseWheelTimer.Stop();

            Application.Current.Dispatcher.BeginInvoke(new Action(() => VolumeButtonPopup.Close()));
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            this.keepOpenAfterScrolling = true;
            this.mouseWheelTimer.Stop();
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            this.mouseWheelTimer.Start();
        }
        #endregion
    }
}
