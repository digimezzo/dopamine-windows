using Dopamine.Core.Audio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Equalizer
{
    public delegate void EqualizerPresetChangedEventhandler(EqualizerPreset Preset);
    public delegate void EqualizerBandChangedEventhandler(int band, double value);
    public delegate void EqualizerIsEnabledChangedEventHandler(bool isEnabled);

    public interface IEqualizerService
    {
        bool IsEnabled { get; set; }
        EqualizerPreset Preset { get; }
        Task<List<EqualizerPreset>> GetEqualizerPresetsAsync();
        void SetEqualizerPreset(EqualizerPreset preset);
        void SetEqualizerBand(int band, double value);

        event EqualizerPresetChangedEventhandler EqualizerPresetChanged;
        event EqualizerBandChangedEventhandler EqualizerBandChanged;
        event EqualizerIsEnabledChangedEventHandler EqualizerIsEnabledChanged;
    }
}
