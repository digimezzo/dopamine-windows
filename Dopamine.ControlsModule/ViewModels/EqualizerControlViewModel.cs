using Dopamine.Common.Services.Equalizer;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
using Dopamine.Core.Audio;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace Dopamine.ControlsModule.ViewModels
{
    public class EqualizerControlViewModel : BindableBase
    {
        #region Variables
        private double slider0Value;
        private double slider1Value;
        private double slider2Value;
        private double slider3Value;
        private double slider4Value;
        private double slider5Value;
        private double slider6Value;
        private double slider7Value;
        private double slider8Value;
        private double slider9Value;

        private IPlaybackService playbackService;
        private IEqualizerService equalizerService;
        #endregion

        #region Properties
        public double Slider0Value
        {
            get { return this.slider0Value; }
            set
            {
                SetProperty<double>(ref this.slider0Value, value);
                this.ApplyEqualizerBand(0, value);
            }
        }

        public double Slider1Value
        {
            get { return this.slider1Value; }
            set
            {
                SetProperty<double>(ref this.slider1Value, value);
                this.ApplyEqualizerBand(1, value);
            }
        }

        public double Slider2Value
        {
            get { return this.slider2Value; }
            set
            {
                SetProperty<double>(ref this.slider2Value, value);
                this.ApplyEqualizerBand(2, value);
            }
        }

        public double Slider3Value
        {
            get { return this.slider3Value; }
            set
            {
                SetProperty<double>(ref this.slider3Value, value);
                this.ApplyEqualizerBand(3, value);
            }
        }

        public double Slider4Value
        {
            get { return this.slider4Value; }
            set
            {
                SetProperty<double>(ref this.slider4Value, value);
                this.ApplyEqualizerBand(4, value);
            }
        }

        public double Slider5Value
        {
            get { return this.slider5Value; }
            set
            {
                SetProperty<double>(ref this.slider5Value, value);
                this.ApplyEqualizerBand(5, value);
            }
        }

        public double Slider6Value
        {
            get { return this.slider6Value; }
            set
            {
                SetProperty<double>(ref this.slider6Value, value);
                this.ApplyEqualizerBand(6, value);
            }
        }

        public double Slider7Value
        {
            get { return this.slider7Value; }
            set
            {
                SetProperty<double>(ref this.slider7Value, value);
                this.ApplyEqualizerBand(7, value);
            }
        }

        public double Slider8Value
        {
            get { return this.slider8Value; }
            set
            {
                SetProperty<double>(ref this.slider8Value, value);
                this.ApplyEqualizerBand(8, value);
            }
        }

        public double Slider9Value
        {
            get { return this.slider9Value; }
            set
            {
                SetProperty<double>(ref this.slider9Value, value);
                this.ApplyEqualizerBand(9, value);
            }
        }
        #endregion

        #region Construction
        public EqualizerControlViewModel(IPlaybackService playbackService, IEqualizerService equalizerService)
        {
            this.playbackService = playbackService;
            this.equalizerService = equalizerService;

            this.LoadFromSettingsAsync();
        }
        #endregion

        #region Private
        private async void LoadFromSettingsAsync()
        {
            await Task.Run(() =>
            {
                EqualizerPreset preset = equalizerService.Preset;

                this.slider0Value = preset.Bands[0];
                this.slider1Value = preset.Bands[1];
                this.slider2Value = preset.Bands[2];
                this.slider3Value = preset.Bands[3];
                this.slider4Value = preset.Bands[4];
                this.slider5Value = preset.Bands[5];
                this.slider6Value = preset.Bands[6];
                this.slider7Value = preset.Bands[7];
                this.slider8Value = preset.Bands[8];
                this.slider9Value = preset.Bands[9];
            });
        }

        private void ApplyEqualizerBand(int band, double value)
        {
            if (this.playbackService != null)
            {
                this.playbackService.SetEqualizerBand(band, value);
            }

            // TODO: also save in settings
        }
        #endregion
    }
}