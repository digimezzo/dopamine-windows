using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Provider
{
    public interface IProviderService
    {
        Task<List<VideoProvider>> GetVideoProvidersAsync();
    }
}
