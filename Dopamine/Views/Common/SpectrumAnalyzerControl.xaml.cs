using CommonServiceLocator;
using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
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

            this.playbackService.PlaybackSuccess += (_, __) => this.TryRegisterSpectrumPlayers();
            this.shellService.WindowStateChanged += (_, __) => this.TryRegisterSpectrumPlayers();

            Digimezzo.Foundation.Core.Settings.SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Playback", "ShowSpectrumAnalyzer"))
                {
                    this.TryRegisterSpectrumPlayers();
                }
            };

            this.TryRegisterSpectrumPlayers();
        }

        private void TryRegisterSpectrumPlayers()
        {
            this.UnregisterSpectrumPlayers();

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
            Application.Current.Dispatcher.Invoke(() => this.SpectrumContainer.Visibility = Visibility.Collapsed);

            if (this.playbackService.Player != null)
            {
                Application.Current.Dispatcher.Invoke(() => this.LeftSpectrumAnalyzer.UnregisterSoundPlayer());
                Application.Current.Dispatcher.Invoke(() => this.RightSpectrumAnalyzer.UnregisterSoundPlayer());
            }
        }
    }
}
