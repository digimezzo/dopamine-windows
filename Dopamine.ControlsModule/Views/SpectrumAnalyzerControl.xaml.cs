using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ServiceLocation;
using System.Windows;
using Dopamine.Common.Enums;

namespace Dopamine.ControlsModule.Views
{
    public partial class SpectrumAnalyzerControl
    {
        #region Variables
        private IPlaybackService playbackService;
        #endregion

        #region Properties
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }
        #endregion

        #region Construction
        public SpectrumAnalyzerControl()
        {
            InitializeComponent();

            this.playbackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) => this.RegisterPlayer();

            // Just in case we switched Views after the playBackService.PlaybackSuccess was triggered
            this.RegisterPlayer();
        }
        #endregion

        #region Private
        private void RegisterPlayer()
        {
            if(this.playbackService.Player != null)
            {
                Application.Current.Dispatcher.Invoke(() => { this.LeftSpectrumAnalyzer.RegisterSoundPlayer(this.playbackService.Player.GetWrapperSpectrumPlayer(SpectrumChannel.Left)); });
                Application.Current.Dispatcher.Invoke(() => { this.RightSpectrumAnalyzer.RegisterSoundPlayer(this.playbackService.Player.GetWrapperSpectrumPlayer(SpectrumChannel.Right)); });
            }
        }
        #endregion
    }
}
