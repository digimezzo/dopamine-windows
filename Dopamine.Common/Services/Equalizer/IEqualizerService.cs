using System.Collections.Generic;
using System.Threading.Tasks;
using Dopamine.Core.Audio;

namespace Dopamine.Common.Services.Equalizer
{
    public interface IEqualizerService
    {
        Task<List<EqualizerPreset>> GetEqualizerPresetsAsync();
    }
}
