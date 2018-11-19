using CommonServiceLocator;
using Dopamine.Core.Enums;
using Dopamine.Services.Playback;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class SpectrumAnalyzerControl : UserControl
    {
        private IPlaybackService playbackService;
       
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }
      
        public SpectrumAnalyzerControl()
        {
            InitializeComponent();

            this.playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            this.playbackService.PlaybackSuccess += (_,__) => this.RegisterSpectrumPlayers();
            this.playbackService.SpectrumVisibilityChanged += isSpectrumVisible =>
            {
                if (isSpectrumVisible)
                {
                    this.RegisterSpectrumPlayers();
                }
                else
                {
                    this.UnregisterSpectrumPlayers();
                }
            };

            // Just in case we switched Views after the playBackService.PlaybackSuccess was triggered
            this.RegisterSpectrumPlayers();
        }
       
        private void RegisterSpectrumPlayers()
        {
            if(this.playbackService.Player != null && (this.playbackService.IsSpectrumVisible))
            {
                Application.Current.Dispatcher.Invoke(() => this.LeftSpectrumAnalyzer.RegisterSoundPlayer(this.playbackService.Player.GetWrapperSpectrumPlayer(SpectrumChannel.Left)));
                Application.Current.Dispatcher.Invoke(() => this.RightSpectrumAnalyzer.RegisterSoundPlayer(this.playbackService.Player.GetWrapperSpectrumPlayer(SpectrumChannel.Right)));
            }
        }

        private void UnregisterSpectrumPlayers()
        {
            if (this.playbackService.Player != null)
            {
                Application.Current.Dispatcher.Invoke(() => this.LeftSpectrumAnalyzer.UnregisterSoundPlayer());
                Application.Current.Dispatcher.Invoke(() => this.RightSpectrumAnalyzer.UnregisterSoundPlayer());
            }
        }
    }
}
