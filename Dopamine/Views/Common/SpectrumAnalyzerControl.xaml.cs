using CommonServiceLocator;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Enums;
using Dopamine.Core.Helpers;
using Dopamine.Services.Playback;
using Dopamine.Services.Shell;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class SpectrumAnalyzerControl : UserControl
    {
        private IPlaybackService playbackService;
        private IShellService shellService;
        private bool isSubscribed;

        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }

        public SpectrumAnalyzerControl()
        {
            InitializeComponent();

            this.playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            this.shellService = ServiceLocator.Current.GetInstance<IShellService>();

            // Subscribe when loaded and release the subscriptions when unloaded, so repeated
            // navigations (full player / mini players) don't leak this control.
            this.Loaded += this.LoadedHandler;
            this.Unloaded += this.UnloadedHandler;
        }

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            if (this.isSubscribed)
            {
                return;
            }

            this.isSubscribed = true;

            this.playbackService.PlaybackSuccess += this.PlaybackSuccessHandler;
            this.shellService.WindowStateChanged += this.WindowStateChangedHandler;
            SettingsClient.SettingChanged += this.SettingChangedHandler;

            this.TryRegisterSpectrumPlayers();
        }

        private void UnloadedHandler(object sender, RoutedEventArgs e)
        {
            if (!this.isSubscribed)
            {
                return;
            }

            this.isSubscribed = false;

            this.playbackService.PlaybackSuccess -= this.PlaybackSuccessHandler;
            this.shellService.WindowStateChanged -= this.WindowStateChangedHandler;
            SettingsClient.SettingChanged -= this.SettingChangedHandler;

            this.UnregisterSpectrumPlayers();
        }

        private void PlaybackSuccessHandler(object sender, PlaybackSuccessEventArgs e)
        {
            this.TryRegisterSpectrumPlayers();
        }

        private void WindowStateChangedHandler(object sender, WindowStateChangedEventArgs e)
        {
            this.TryRegisterSpectrumPlayers();
        }

        private void SettingChangedHandler(object sender, SettingChangedEventArgs e)
        {
            if (SettingsClient.IsSettingChanged(e, "Playback", "ShowSpectrumAnalyzer"))
            {
                this.TryRegisterSpectrumPlayers();
            }
        }

        private void TryRegisterSpectrumPlayers()
        {
            this.UnregisterSpectrumPlayers();

            if (Application.Current == null)
            {
                return;
            }

            if (!this.playbackService.HasMediaFoundationSupport)
            {
                return;
            }

            if (!SettingsClient.Get<bool>("Playback", "ShowSpectrumAnalyzer"))
            {
                // The settings don't allow showing the spectrum analyzer
                return;
            }

            if (this.shellService.WindowState == WindowState.Minimized)
            {
                // The window state doesn't allow showing the spectrum analyzer
                return;
            }

            Application.Current.Dispatcher.Invoke(() => this.SpectrumContainer.Visibility = Visibility.Visible);

            if (this.playbackService.Player != null)
            {
                Application.Current.Dispatcher.Invoke(() => this.LeftSpectrumAnalyzer.RegisterSoundPlayer(this.playbackService.Player.GetWrapperSpectrumPlayer(SpectrumChannel.Left)));
                Application.Current.Dispatcher.Invoke(() => this.RightSpectrumAnalyzer.RegisterSoundPlayer(this.playbackService.Player.GetWrapperSpectrumPlayer(SpectrumChannel.Right)));
            }
        }

        private void UnregisterSpectrumPlayers()
        {
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() => this.SpectrumContainer.Visibility = Visibility.Collapsed);

            if (this.playbackService.Player != null)
            {
                Application.Current.Dispatcher.Invoke(() => this.LeftSpectrumAnalyzer.UnregisterSoundPlayer());
                Application.Current.Dispatcher.Invoke(() => this.RightSpectrumAnalyzer.UnregisterSoundPlayer());
            }
        }
    }
}
