using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Equalizer
{
    public interface IEqualizerService
    {
        Task<List<EqualizerPreset>> GetEqualizerPresetsAsync();
    }
}
