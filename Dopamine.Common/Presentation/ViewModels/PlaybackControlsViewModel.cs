using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Settings;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Timers;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackControlsViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private IEventAggregator eventAggregator;
        private bool showPause;
        private LoopMode loopMode;
        private bool shuffle;
        private bool isLoadingTrack;
        private Timer isLoadingTrackTimer = new Timer();
        #endregion

        #region Commands
        public DelegateCommand PauseCommand { get; set; }
        public DelegateCommand PreviousCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand LoopCommand { get; set; }
        public DelegateCommand ShuffleCommand { get; set; }
        public DelegateCommand PlayCommand { get; set; }
        #endregion

        #region Properties
        public bool ShowPause
        {
            get { return this.showPause; }
            set { SetProperty<bool>(ref this.showPause, value); }
        }

        public bool ShowLoopNone
        {
            get { return this.loopMode == LoopMode.None; }
        }

        public bool ShowLoopOne
        {
            get { return this.loopMode == LoopMode.One; }
        }

        public bool ShowLoopAll
        {
            get { return this.loopMode == LoopMode.All; }
        }

        public bool Shuffle
        {
            get { return this.shuffle; }

            set
            {
                // Empty on purpose. OnPropertyChanged is fired in GetPlayBackServiceShuffle.
            }
        }

        public bool IsLoadingTrack
        {
            get { return this.isLoadingTrack; }
            set { SetProperty<bool>(ref this.isLoadingTrack, value); }
        }
        #endregion

        #region Construction
        public PlaybackControlsViewModel(IPlaybackService playbackService, IEventAggregator eventAggregator)
        {
            // Injection
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;

            // Timers
            this.isLoadingTrackTimer.Interval = 1000;
            this.isLoadingTrackTimer.Elapsed += IsLoadingTrackTimer_Elapsed;

            // Commands
            this.PauseCommand = new DelegateCommand(() => this.playbackService.PlayOrPauseAsync());
            this.PreviousCommand = new DelegateCommand(async () => await this.playbackService.PlayPreviousAsync());
            this.NextCommand = new DelegateCommand(async () => await this.playbackService.PlayNextAsync());
            this.LoopCommand = new DelegateCommand(() => this.SetPlayBackServiceLoop());
            this.ShuffleCommand = new DelegateCommand(() => this.SetPlayBackServiceShuffle(!this.shuffle));
            this.PlayCommand = new DelegateCommand(async () => await this.playbackService.PlayOrPauseAsync());

            // Event handlers
            this.playbackService.PlaybackFailed += (sender, e) => this.ShowPause = false;
            this.playbackService.PlaybackPaused += (sender, e) => this.ShowPause = false;
            this.playbackService.PlaybackResumed += (sender, e) => this.ShowPause = true;
            this.playbackService.PlaybackStopped += (sender, e) => this.ShowPause = false;
            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) => this.ShowPause = true;
            this.playbackService.PlaybackLoopChanged += (sender, e) => this.GetPlayBackServiceLoop();
            this.playbackService.PlaybackShuffleChanged += (sender, e) => this.GetPlayBackServiceShuffle();

            this.playbackService.LoadingTrack += (isLoadingTrack) =>
            {
                if (isLoadingTrack)
                {
                    this.isLoadingTrackTimer.Stop();
                    this.isLoadingTrackTimer.Start();
                }
                else
                {
                    this.isLoadingTrackTimer.Stop();
                    this.IsLoadingTrack = false;
                }
            };

            // Initial Loop and Shuffle state
            this.loopMode = (LoopMode)XmlSettingsClient.Instance.Get<int>("Playback", "LoopMode");
            this.shuffle = XmlSettingsClient.Instance.Get<bool>("Playback", "Shuffle");

            // Initial status of the Play/Pause button
            if (this.playbackService.IsPlaying)
            {
                this.ShowPause = true;
            }
            else
            {
                this.ShowPause = false;
            }
        }
        #endregion

        #region Event Handlers
        private void IsLoadingTrackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.IsLoadingTrack = true;
        }
        #endregion

        #region Private
        private void SetPlayBackServiceLoop()
        {
            switch (this.loopMode)
            {
                case LoopMode.None:
                    this.playbackService.LoopMode = LoopMode.All;
                    break;
                case LoopMode.All:
                    this.playbackService.LoopMode = LoopMode.One;
                    break;
                case LoopMode.One:
                    this.playbackService.LoopMode = LoopMode.None;
                    break;
                default:
                    this.playbackService.LoopMode = LoopMode.None;
                    break;
            }

            // Loop and Shuffle cannot be enabled at the same time
            if (this.playbackService.LoopMode != LoopMode.None)
            {
                this.playbackService.SetShuffle(false);
            }
        }

        private void SetPlayBackServiceShuffle(bool iShuffle)
        {
            this.playbackService.SetShuffle(iShuffle);

            // Loop and Shuffle cannot be enabled at the same time
            if (iShuffle)
            {
                this.playbackService.LoopMode = LoopMode.None;
            }
        }

        public void GetPlayBackServiceLoop()
        {
            // Important: set Loop directly, not the Loop Property, 
            // because there is no Loop Property Setter!
            this.loopMode = this.playbackService.LoopMode;

            OnPropertyChanged(() => this.ShowLoopNone);
            OnPropertyChanged(() => this.ShowLoopOne);
            OnPropertyChanged(() => this.ShowLoopAll);

            // Save the Loop status in the Settings
            XmlSettingsClient.Instance.Set<int>("Playback", "LoopMode", (int)this.loopMode);
        }

        public void GetPlayBackServiceShuffle()
        {
            // Important: set Shuffle directly, not the Shuffle Property, 
            // because there is no Shuffle Property Setter!
            this.shuffle = this.playbackService.Shuffle;

            OnPropertyChanged(() => this.Shuffle);

            // Save the Shuffle status in the Settings
            XmlSettingsClient.Instance.Set<bool>("Playback", "Shuffle", this.shuffle);
        }
        #endregion
    }
}
