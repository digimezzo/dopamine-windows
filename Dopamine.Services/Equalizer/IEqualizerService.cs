using Dopamine.Core.Audio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Equalizer
{
    public interface IEqualizerService
    {
        Task<List<EqualizerPreset>> GetPresetsAsync();
        Task<EqualizerPreset> GetSelectedPresetAsync();
    }
}
