using System;

namespace Dopamine.Services.Contracts.Search
{
    public interface ISearchService
    {
        string SearchText { get; set; }
        event Action<string> DoSearch;
    }
}
