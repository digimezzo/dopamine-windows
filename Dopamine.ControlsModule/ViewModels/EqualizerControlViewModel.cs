using Dopamine.Common.Services.Equalizer;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Audio;
using Dopamine.Core.Settings;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Dopamine.ControlsModule.ViewModels
{
    public class EqualizerControlViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private IEqualizerService equalizerService;

        private ObservableCollection<EqualizerPreset> presets;
        private EqualizerPreset selectedPreset;
        private bool isEqualizerEnabled;
        #endregion

        #region Properties
        public ObservableCollection<EqualizerPreset> Presets
        {
            get { return this.presets; }
            set
            {
                SetProperty<ObservableCollection<EqualizerPreset>>(ref this.presets, value);
            }
        }

        public EqualizerPreset SelectedPreset
        {
            get { return this.selectedPreset; }
            set
            {
                SetProperty<EqualizerPreset>(ref this.selectedPreset, value);
                XmlSettingsClient.Instance.Set<string>("Equalizer", "SelectedPreset", value.Name);
                this.playbackService.SwitchPreset(ref value); // Make sure that playbackService has a reference to the selected preset
            }
        }

        public bool IsEqualizerEnabled
        {
            get { return this.isEqualizerEnabled; }
            set
            {
                SetProperty<bool>(ref this.isEqualizerEnabled, value);
                XmlSettingsClient.Instance.Set<bool>("Equalizer", "IsEnabled", value);
                this.playbackService.SetEqualizerEnabledState(value);
            }
        }
        #endregion

        #region Construction
        public EqualizerControlViewModel(IPlaybackService playbackService, IEqualizerService equalizerService)
        {
            this.playbackService = playbackService;
            this.equalizerService = equalizerService;

            this.IsEqualizerEnabled = XmlSettingsClient.Instance.Get<bool>("Equalizer", "IsEnabled");

            this.InitializeAsync();
        }
        #endregion

        #region Private
        private async void InitializeAsync()
        {
            ObservableCollection<EqualizerPreset> localEqualizerPresets = new ObservableCollection<EqualizerPreset>();

            foreach (EqualizerPreset preset in await this.equalizerService.GetPresetsAsync())
            {
                localEqualizerPresets.Add(preset);
            }

            this.Presets = localEqualizerPresets;

            this.selectedPreset = await this.equalizerService.GetSelectedPresetAsync(); // Don't use the SelectedPreset setter directly, because that saves SelectedPreset to the settings file.
            this.playbackService.SwitchPreset(ref this.selectedPreset); // Make sure that playbackService has a reference to the selected preset
            OnPropertyChanged(() => this.SelectedPreset);
        }
        #endregion
    }
}