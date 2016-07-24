using System;

namespace Dopamine.Common.Services.Search
{
    public interface ISearchService
    {
        string SearchText { get; set; }
        event Action<string> DoSearch;
    }
}
