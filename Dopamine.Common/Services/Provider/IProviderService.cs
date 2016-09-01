using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Provider
{
    public interface IProviderService
    {
        Task<List<SearchProvider>> GetSearchProvidersAsync();
        void SearchOnline(string id, string[] searchArguments);
        bool RemoveSearchProvider(SearchProvider provider);
        bool UpdateSearchProvider(SearchProvider provider);
        event EventHandler SearchProvidersChanged;
    }
}
