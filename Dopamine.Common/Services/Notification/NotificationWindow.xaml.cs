using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.IO;
using Dopamine.Core.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Dopamine.Common.Services.Notification
{
    public partial class NotificationWindow : Window
    {
        #region Variables
        private DispatcherTimer hideTimer = new DispatcherTimer();
        private DispatcherTimer closeTimer = new DispatcherTimer();
        private int maxSecondsVisible = 5;
        private int hideTimerValue = 1;
        private bool isMouseOver = false;
        private bool draggingVolume;
        private float initialVolume;
        private int notificationShadowSize = 10;
        private int notificationMarginFromScreen = 0;
        #endregion

        #region Construction
        public NotificationWindow(MergedTrack track, byte[] artworkData, NotificationPosition position, bool showControls, int maxSecondsVisible) : base()
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
                this.TextBoxTitle.Text = ResourceUtils.GetStringResource("Language_Title");
                this.TextBoxArtist.Text = ResourceUtils.GetStringResource("Language_Artist");
            }

            this.ToolTipTitle.Text = this.TextBoxTitle.Text;
            this.ToolTipArtist.Text = this.TextBoxArtist.Text;

            if (artworkData != null)
            {
                try
                {
                    // Width and Height are 300px. They need to be big enough, otherwise the picture is blurry
                    this.CoverPicture.Source = ImageOperations.ByteToBitmapImage(artworkData, 300, 300);
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
        #endregion

        #region Event Handlers
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
            WindowUtils.HideWindowFromAltTab(this);
            this.IsEnabled = true;
            // This activates fade-in
            this.hideTimer.Start();
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
        #endregion

        #region Public
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
        #endregion

        #region Events
        public event EventHandler DoubleClicked = delegate { };
        #endregion
    }
}
