using Dopamine.Common.Services.Playback;
using Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class EqualizerControlViewModel : BindableBase
    {
        #region Variables
        private double slider1Value;
        private double slider2Value;
        private double slider3Value;
        private double slider4Value;
        private double slider5Value;
        private double slider6Value;
        private double slider7Value;
        private double slider8Value;
        private double slider9Value;
        private double slider10Value;
        private IPlaybackService playbackService;
        #endregion

        #region Properties
        public double Slider1Value
        {
            get { return this.slider1Value; }
            set
            {
                SetProperty<double>(ref this.slider1Value, value);
                this.ApplyEqualizerValue(0, value);
            }
        }

        public double Slider2Value
        {
            get { return this.slider2Value; }
            set
            {
                SetProperty<double>(ref this.slider2Value, value);
                this.ApplyEqualizerValue(1, value);
            }
        }

        public double Slider3Value
        {
            get { return this.slider3Value; }
            set
            {
                SetProperty<double>(ref this.slider3Value, value);
                this.ApplyEqualizerValue(2, value);
            }
        }

        public double Slider4Value
        {
            get { return this.slider4Value; }
            set
            {
                SetProperty<double>(ref this.slider4Value, value);
                this.ApplyEqualizerValue(3, value);
            }
        }

        public double Slider5Value
        {
            get { return this.slider5Value; }
            set
            {
                SetProperty<double>(ref this.slider5Value, value);
                this.ApplyEqualizerValue(4, value);
            }
        }

        public double Slider6Value
        {
            get { return this.slider6Value; }
            set
            {
                SetProperty<double>(ref this.slider6Value, value);
                this.ApplyEqualizerValue(5, value);
            }
        }

        public double Slider7Value
        {
            get { return this.slider7Value; }
            set
            {
                SetProperty<double>(ref this.slider7Value, value);
                this.ApplyEqualizerValue(6, value);
            }
        }

        public double Slider8Value
        {
            get { return this.slider8Value; }
            set
            {
                SetProperty<double>(ref this.slider8Value, value);
                this.ApplyEqualizerValue(7, value);
            }
        }

        public double Slider9Value
        {
            get { return this.slider9Value; }
            set
            {
                SetProperty<double>(ref this.slider9Value, value);
                this.ApplyEqualizerValue(8, value);
            }
        }

        public double Slider10Value
        {
            get { return this.slider10Value; }
            set
            {
                SetProperty<double>(ref this.slider10Value, value);
                this.ApplyEqualizerValue(9, value);
            }
        }
        #endregion

        #region Construction
        public EqualizerControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            // TODO: these values should come from settings
            this.Slider1Value = 0.5;
            this.Slider2Value = 0.5;
            this.Slider3Value = 0.5;
            this.Slider4Value = 0.5;
            this.Slider5Value = 0.5;
            this.Slider6Value = 0.5;
            this.Slider7Value = 0.5;
            this.Slider8Value = 0.5;
            this.Slider9Value = 0.5;
            this.Slider10Value = 0.5;
        }
        #endregion

        #region Private
        private void ApplyEqualizerValue(int index, double sliderValue)
        {
            if (this.playbackService != null)
            {
                this.playbackService.UpdateEqualizer(index, sliderValue);
            }

            // TODO: also save in settings
        }
        #endregion
    }
}