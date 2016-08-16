using Dopamine.Core.Audio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Equalizer
{
    public delegate void EqualizerIsEnabledChangedEventHandler(bool isEnabled);

    public interface IEqualizerService
    {
        bool IsEnabled { get; set; }
        Task<List<EqualizerPreset>> GetEqualizerPresetsAsync();
        event EqualizerIsEnabledChangedEventHandler EqualizerIsEnabledChanged;
    }
}
