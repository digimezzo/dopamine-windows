using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Provider
{
    public interface IProviderService
    {
        Task<List<SearchProvider>> GetSearchProvidersAsync();
        void SearchOnline(string id, string[] searchArguments);
        bool RemoveSearchProvider(SearchProvider provider);
        UpdateSearchProviderResult AddSearchProvider(SearchProvider provider);
        UpdateSearchProviderResult UpdateSearchProvider(SearchProvider provider);
        event EventHandler SearchProvidersChanged;
    }
}
