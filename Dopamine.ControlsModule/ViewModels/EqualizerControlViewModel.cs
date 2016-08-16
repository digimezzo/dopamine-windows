using Dopamine.Common.Services.Equalizer;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Audio;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dopamine.ControlsModule.ViewModels
{
    public class EqualizerControlViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private IEqualizerService equalizerService;

        private ObservableCollection<EqualizerPreset> presets;
        private EqualizerPreset selectedPreset;
        #endregion

        #region Properties
        public IEqualizerService EqualizerService
        {
            get { return this.equalizerService; }
        }
        
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
            }
        }
        #endregion

        #region Construction
        public EqualizerControlViewModel(IPlaybackService playbackService, IEqualizerService equalizerService)
        {
            this.playbackService = playbackService;
            this.equalizerService = equalizerService;

            this.InitializeAsync();
        }
        #endregion

        #region Private
        private async void InitializeAsync()
        {
            ObservableCollection<EqualizerPreset> localEqualizerPresets = new ObservableCollection<EqualizerPreset>();

            foreach (EqualizerPreset preset in await this.equalizerService.GetEqualizerPresetsAsync())
            {
                localEqualizerPresets.Add(preset);
            }

            this.Presets = localEqualizerPresets;

            this.SelectedPreset = this.Presets.First();
        }
        #endregion
    }
}