using Digimezzo.Utilities.Settings;
using Dopamine.Controls;
using Dopamine.Core.Enums;
using Dopamine.Services.Appearance;
using Dopamine.Services.Playback;
using Prism.Events;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Media;

namespace Dopamine.ViewModels.Common
{
    public class SpectrumAnalyzerControlViewModel : BindableBase
    {
        private IPlaybackService playbackService;
        private IAppearanceService appearanceService;
        private IEventAggregator eventAggregator;
        private bool showSpectrumAnalyzer;
        private bool isPlaying;
        private double blurRadius;
        private int spectrumBarCount;
        private double spectrumWidth;
        private double spectrumBarWidth;
        private double spectrumBarSpacing;
        private double spectrumPanelHeight;
        private double spectrumOpacity;
        private SpectrumAnimationStyle animationStyle;
        private Brush spectrumBarBackground;
        private SpectrumStyle spectrumStyle;
      
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
            set
            {
                SetProperty<double>(ref this.spectrumWidth, value);
                RaisePropertyChanged(nameof(this.SpectrumPanelWidth));
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

        public Brush SpectrumBarBackground
        {
            get { return this.spectrumBarBackground; }
            set { SetProperty<Brush>(ref this.spectrumBarBackground, value); }
        }

        public SpectrumStyle SpectrumStyle
        {
            get { return this.spectrumStyle; }
            set { SetProperty<SpectrumStyle>(ref this.spectrumStyle, value); }
        }
      
        public SpectrumAnalyzerControlViewModel(IPlaybackService playbackService, IAppearanceService appearanceService, IEventAggregator eventAggregator)
        {
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;
            this.appearanceService = appearanceService;

            this.appearanceService.ColorSchemeChanged += (_, __) =>
            Application.Current.Dispatcher.Invoke(() => this.SetSpectrumStyle((SpectrumStyle)SettingsClient.Get<int>("Playback", "SpectrumStyle")));

            this.playbackService.PlaybackFailed += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackStopped += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackPaused += (_, __) => this.IsPlaying = false;
            this.playbackService.PlaybackResumed += (_, __) => this.IsPlaying = true;
            this.playbackService.PlaybackSuccess += (_,__) => this.IsPlaying = true;

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Playback", "SpectrumStyle"))
                {
                    this.SetSpectrumStyle((SpectrumStyle)e.SettingValue);
                }
            };

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
     
        private void SetSpectrumStyle(SpectrumStyle style)
        {
            switch (style)
            {
                case SpectrumStyle.Flames:
                    this.SpectrumStyle = SpectrumStyle.Flames;
                    this.BlurRadius = 20;
                    this.SpectrumBarCount = 65;
                    this.SpectrumWidth = 270;
                    this.SpectrumBarWidth = 4;
                    this.SpectrumBarSpacing = 0;
                    this.SpectrumPanelHeight = 60;
                    this.SpectrumOpacity = 0.65;
                    this.AnimationStyle = SpectrumAnimationStyle.Gentle;
                    //var accentColor = (Color)Application.Current.TryFindResource("Color_Accent");
                    //var gradientColor = HSLColor.GetFromRgb(accentColor).MoveNext(40).ToRgb();
                    //this.SpectrumBarBackground = new LinearGradientBrush(new GradientStopCollection()
                    //{
                    //    new GradientStop(accentColor, 0),
                    //    new GradientStop(accentColor, 0.45),
                    //    new GradientStop(gradientColor, 1),
                    //}, new Point(0.5, 1), new Point(0.5, 0));
                    this.SpectrumBarBackground = (Brush)Application.Current.TryFindResource("Brush_Accent");
                    break;
                case SpectrumStyle.Lines:
                    this.SpectrumStyle = SpectrumStyle.Lines;
                    this.BlurRadius = 0;
                    this.SpectrumBarCount = 50;
                    this.SpectrumWidth = 162;
                    this.SpectrumBarWidth = 1;
                    this.SpectrumBarSpacing = 2;
                    this.SpectrumPanelHeight = 30;
                    this.SpectrumOpacity = 1.0;
                    this.AnimationStyle = SpectrumAnimationStyle.Nervous;
                    this.SpectrumBarBackground = (Brush)Application.Current.TryFindResource("Brush_Accent");
                    break;
                case SpectrumStyle.Bars:
                    this.SpectrumStyle = SpectrumStyle.Bars;
                    this.BlurRadius = 0;
                    this.SpectrumBarCount = 20;
                    this.SpectrumWidth = 162;
                    this.SpectrumBarWidth = 6;
                    this.SpectrumBarSpacing = 2;
                    this.SpectrumPanelHeight = 30;
                    this.SpectrumOpacity = 1.0;
                    this.AnimationStyle = SpectrumAnimationStyle.Nervous;
                    this.SpectrumBarBackground = (Brush)Application.Current.TryFindResource("Brush_Accent");
                    break;
                case SpectrumStyle.Stripes:
                    this.SpectrumStyle = SpectrumStyle.Stripes;
                    this.BlurRadius = 0;
                    this.SpectrumBarCount = 13;
                    this.SpectrumWidth = 162;
                    this.SpectrumBarWidth = 10;
                    this.SpectrumBarSpacing = 2;
                    this.SpectrumPanelHeight = 30;
                    this.SpectrumOpacity = 1.0;
                    this.AnimationStyle = SpectrumAnimationStyle.Nervous;
                    this.SpectrumBarBackground = (Brush)Application.Current.TryFindResource("Brush_AccentStriped");
                    break;
                default:
                    // Shouldn't happen
                    break;
            }

            SettingsClient.Set<int>("Playback", "SpectrumStyle", (int)style);
        }
    }
}
