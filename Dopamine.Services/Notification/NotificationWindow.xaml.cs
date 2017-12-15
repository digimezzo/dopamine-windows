using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dopamine.Services.Notification
{
    public partial class NotificationWindow : Window
    {
        private DispatcherTimer hideTimer = new DispatcherTimer();
        private DispatcherTimer closeTimer = new DispatcherTimer();
        private int maxSecondsVisible = 5;
        private int hideTimerValue = 1;
        private int notificationShadowSize = 10;
        private int notificationMarginFromScreen = 0;
       
        public NotificationWindow(PlayableTrack track, byte[] artworkData, NotificationPosition position, bool showControls, int maxSecondsVisible) : base()
        {
            this.InitializeComponent();

            this.maxSecondsVisible = maxSecondsVisible;

            int notificationHeight = 66 + 2 * this.notificationShadowSize;
            int notificationWidth = 286 + 2 * this.notificationShadowSize;

            if (showControls)
            {
                notificationHeight += 40;
                notificationWidth += 90;
                this.ControlsPanel.Visibility = Visibility.Visible;
                this.VolumePanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.ControlsPanel.Visibility = Visibility.Collapsed;
                this.VolumePanel.Visibility = Visibility.Collapsed;
            }

            if (track != null)
            {
                this.TextBoxTitle.Text = string.IsNullOrEmpty(track.TrackTitle) ? track.FileName : track.TrackTitle;
                this.TextBoxArtist.Text = track.ArtistName;
            }
            else
            {
                this.TextBoxTitle.Text = ResourceUtils.GetString("Language_Title");
                this.TextBoxArtist.Text = ResourceUtils.GetString("Language_Artist");
            }

            this.ToolTipTitle.Text = this.TextBoxTitle.Text;
            this.ToolTipArtist.Text = this.TextBoxArtist.Text;

            if (artworkData != null)
            {
                try
                {
                    // Width and Height are 300px. They need to be big enough, otherwise the picture is blurry
                    this.CoverPicture.Source = ImageUtils.ByteToBitmapImage(artworkData, 300, 300,0);
                    this.CloseBorder.Opacity = 1.0;
                }
                catch (Exception)
                {
                    // Swallow
                }
            }
            else
            {
                this.CloseBorder.Opacity = 0.4;
            }


            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                Rect desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

                // First, set the position
                switch (position)
                {
                    case NotificationPosition.BottomLeft:
                        // Bottom left
                        this.Left = desktopWorkingArea.Left + this.notificationMarginFromScreen;
                        this.Top = desktopWorkingArea.Bottom - notificationHeight - this.notificationMarginFromScreen;
                        break;
                    case NotificationPosition.TopLeft:
                        // Top left
                        this.Left = desktopWorkingArea.Left + this.notificationMarginFromScreen;
                        this.Top = desktopWorkingArea.Top + this.notificationMarginFromScreen;
                        break;
                    case NotificationPosition.TopRight:
                        // Top right
                        this.Left = desktopWorkingArea.Right - notificationWidth - this.notificationMarginFromScreen;
                        this.Top = desktopWorkingArea.Top + this.notificationMarginFromScreen;
                        break;
                    case NotificationPosition.BottomRight:
                        // Bottom right
                        this.Left = desktopWorkingArea.Right - notificationWidth - this.notificationMarginFromScreen;
                        this.Top = desktopWorkingArea.Bottom - notificationHeight - this.notificationMarginFromScreen;
                        break;
                    default:
                        break;
                }

                // After setting the position, set the size. The original size op 0px*0px prevents
                // flicker when displaying the popup. Because it briefly appears in the top left 
                // corner before getting its desired position.
                this.Width = notificationWidth;
                this.Height = notificationHeight;
                this.CoverPicture.Width = notificationHeight - 2 * this.notificationShadowSize;
                this.CoverPicture.Height = notificationHeight - 2 * this.notificationShadowSize;
                this.CoverTile.Width = notificationHeight - 2 * this.notificationShadowSize;
                this.CoverTile.Height = notificationHeight - 2 * this.notificationShadowSize;
            }));

            this.hideTimer.Interval = TimeSpan.FromSeconds(1);
            this.closeTimer.Interval = TimeSpan.FromSeconds(1);
            this.hideTimer.Tick += new EventHandler(HideTimer_Tick);
            this.closeTimer.Tick += new EventHandler(CloseTimer_Tick);
        }
    
        private void HideTimer_Tick(object sender, EventArgs e)
        {

            if (this.hideTimerValue < this.maxSecondsVisible)
            {
                this.hideTimerValue += 1;
            }
            else
            {
                if (!this.IsMouseOver)
                {
                    this.Disable();
                }
            }
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            this.closeTimer.Stop();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = false;

            try
            {
                WindowUtils.HideWindowFromAltTab(this);
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not hide notification window from ALT-TAB menu. Exception: {0}", ex.Message);
            }
            
            this.IsEnabled = true;
            this.hideTimer.Start(); // This activates fade-in
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.hideTimer.Tick -= new EventHandler(HideTimer_Tick);
            this.closeTimer.Tick -= new EventHandler(CloseTimer_Tick);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.DoubleClicked(this, new EventArgs());
            }
        }
    
        public new void Show()
        {
            base.Show();
        }

        public void Disable()
        {
            this.hideTimer.Stop();
            this.IsEnabled = false; // This activates fade-out
            this.closeTimer.Start(); // Only close 1 second after fade-out was activated
        }
      
        public event EventHandler DoubleClicked = delegate { };
    }
}
