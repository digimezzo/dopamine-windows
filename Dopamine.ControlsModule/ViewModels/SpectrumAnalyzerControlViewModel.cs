using Digimezzo.Utilities.Settings;
using Dopamine.Common.Audio;
using Dopamine.Common.Enums;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Playback;
using Prism.Events;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class SpectrumAnalyzerControlViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private IEventAggregator eventAggregator;
        private bool showSpectrumAnalyzer;
        private bool isPlaying;
        private double blurRadius;
        private int spectrumBarCount;
        private double spectrumWidth ;
        private double spectrumBarWidth;
        private double spectrumBarSpacing;
        private double spectrumPanelHeight;
        private double spectrumOpacity;
        private SpectrumAnimationStyle animationStyle;
        #endregion

        #region Properties
        public bool ShowSpectrumAnalyzer
        {
            get { return this.showSpectrumAnalyzer; }
            set { SetProperty<bool>(ref this.showSpectrumAnalyzer, value); }
        }

        public bool IsPlaying
        {
            get { return this.isPlaying; }
            set { SetProperty<bool>(ref this.isPlaying, value); }
        }

        public double BlurRadius
        {
            get { return this.blurRadius; }
            set { SetProperty<double>(ref this.blurRadius, value); }
        }

        public int SpectrumBarCount
        {
            get { return this.spectrumBarCount; }
            set { SetProperty<int>(ref this.spectrumBarCount, value); }
        }

        public double SpectrumWidth
        {
            get { return this.spectrumWidth; }
            set {
                SetProperty<double>(ref this.spectrumWidth, value);
                OnPropertyChanged(() => this.SpectrumPanelWidth);
            }
        }

        public double SpectrumBarWidth
        {
            get { return this.spectrumBarWidth; }
            set { SetProperty<double>(ref this.spectrumBarWidth, value); }
        }

        public double SpectrumBarSpacing
        {
            get { return this.spectrumBarSpacing; }
            set { SetProperty<double>(ref this.spectrumBarSpacing, value); }
        }

        public double SpectrumPanelWidth
        {
            get { return this.SpectrumWidth * 2; }
        }

        public double SpectrumPanelHeight
        {
            get { return this.spectrumPanelHeight; }
            set { SetProperty<double>(ref this.spectrumPanelHeight, value); }
        }

        public double SpectrumOpacity
        {
            get { return this.spectrumOpacity; }
            set { SetProperty<double>(ref this.spectrumOpacity, value); }
        }

        public SpectrumAnimationStyle AnimationStyle
        {
            get { return this.animationStyle; }
            set { SetProperty<SpectrumAnimationStyle>(ref this.animationStyle, value); }
        }
        #endregion

        #region Construction
        public SpectrumAnalyzerControlViewModel(IPlaybackService playbackService, IEventAggregator eventAggregator)
        {
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;

            this.playbackService.SpectrumVisibilityChanged += isSpectrumVisible => this.ShowSpectrumAnalyzer = isSpectrumVisible;

            this.playbackService.PlaybackFailed += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackStopped += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackPaused += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackResumed += (_, __) => this.IsPlaying = true;
            this.playbackService.PlaybackSuccess += (_) => this.IsPlaying = true;

            this.eventAggregator.GetEvent<SettingSpectrumStyleChanged>().Subscribe((spectrumStyle) => this.SetSpectrumStyle(spectrumStyle));

            this.ShowSpectrumAnalyzer = SettingsClient.Get<bool>("Playback", "ShowSpectrumAnalyzer");

            // Initial value
            if (!this.playbackService.IsStopped & this.playbackService.IsPlaying)
            {
                this.IsPlaying = true;
            }
            else
            {
                this.IsPlaying = false;
            }   

            // Default spectrum
            this.SetSpectrumStyle((SpectrumStyle)SettingsClient.Get<int>("Playback", "SpectrumStyle"));
        }
        #endregion

        #region Private
        private void SetSpectrumStyle(SpectrumStyle style)
        {
            switch (style)
            {
                case SpectrumStyle.Fire:
                    this.BlurRadius = 20;
                    this.SpectrumBarCount = 65;
                    this.SpectrumWidth = 270;
                    this.SpectrumBarWidth = 4;
                    this.SpectrumBarSpacing = 0;
                    this.SpectrumPanelHeight = 60;
                    this.SpectrumOpacity = 0.65;
                    this.AnimationStyle = SpectrumAnimationStyle.Gentle;
                    break;
                case SpectrumStyle.Lines:
                    this.BlurRadius = 0;
                    this.SpectrumBarCount = 45;
                    this.SpectrumWidth = 175;
                    this.SpectrumBarWidth = 1;
                    this.SpectrumBarSpacing = 2;
                    this.SpectrumPanelHeight = 30;
                    this.SpectrumOpacity = 1.0;
                    this.AnimationStyle = SpectrumAnimationStyle.Nervous;
                    break;
                case SpectrumStyle.Bars:
                    this.BlurRadius = 0;
                    this.SpectrumBarCount = 21;
                    this.SpectrumWidth = 175;
                    this.SpectrumBarWidth = 6;
                    this.SpectrumBarSpacing = 2;
                    this.SpectrumPanelHeight = 30;
                    this.SpectrumOpacity = 1.0;
                    this.AnimationStyle = SpectrumAnimationStyle.Nervous;
                    break;
                default:
                    // Shouldn't happen
                    break;
            }
        }
        #endregion
    }
}
