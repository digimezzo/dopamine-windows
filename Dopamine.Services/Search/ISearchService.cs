using System;

namespace Dopamine.Services.Search
{
    public interface ISearchService
    {
        string SearchText { get; set; }
        event Action<string> DoSearch;
    }
}
