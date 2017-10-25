using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Base;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Timers;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackControlsViewModel : BindableBase
    {
        private IPlaybackService playbackService;
        private IEventAggregator eventAggregator;
        private bool showPause;
        private LoopMode loopMode;
        private bool shuffle;
        private bool isLoadingTrack;
        private Timer isLoadingTrackTimer = new Timer();
   
        public DelegateCommand PauseCommand { get; set; }
        public DelegateCommand PreviousCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand LoopCommand { get; set; }
        public DelegateCommand ShuffleCommand { get; set; }
        public DelegateCommand PlayCommand { get; set; }
       
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
            this.playbackService.PlaybackFailed += (_,__) => this.ShowPause = false;
            this.playbackService.PlaybackPaused += (_, __) => this.ShowPause = false;
            this.playbackService.PlaybackResumed += (_, __) => this.ShowPause = true;
            this.playbackService.PlaybackStopped += (_, __) => this.ShowPause = false;
            this.playbackService.PlaybackSuccess += (isPlayingPreviousTrack) => this.ShowPause = true;
            this.playbackService.PlaybackLoopChanged += (_, __) => this.GetPlayBackServiceLoop();
            this.playbackService.PlaybackShuffleChanged += (_, __) => this.GetPlayBackServiceShuffle();

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
            this.loopMode = (LoopMode)SettingsClient.Get<int>("Playback", "LoopMode");
            this.shuffle = SettingsClient.Get<bool>("Playback", "Shuffle");

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
    
        private void IsLoadingTrackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.IsLoadingTrack = true;
        }
    
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
        }

        private void SetPlayBackServiceShuffle(bool iShuffle)
        {
            this.playbackService.SetShuffleAsync(iShuffle);
        }

        public void GetPlayBackServiceLoop()
        {
            // Important: set Loop directly, not the Loop Property, 
            // because there is no Loop Property Setter!
            this.loopMode = this.playbackService.LoopMode;

            RaisePropertyChanged(nameof(this.ShowLoopNone));
            RaisePropertyChanged(nameof(this.ShowLoopOne));
            RaisePropertyChanged(nameof(this.ShowLoopAll));

            // Save the Loop status in the Settings
            SettingsClient.Set<int>("Playback", "LoopMode", (int)this.loopMode);
        }

        public void GetPlayBackServiceShuffle()
        {
            // Important: set Shuffle directly, not the Shuffle Property, 
            // because there is no Shuffle Property Setter!
            this.shuffle = this.playbackService.Shuffle;

            RaisePropertyChanged(nameof(this.Shuffle));

            // Save the Shuffle status in the Settings
            SettingsClient.Set<bool>("Playback", "Shuffle", this.shuffle);
        }
    }
}