using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Indexing
{
    public interface IIndexingService
    {
        void OnFoldersChanged();
        bool IsIndexing { get; }
        Task CheckCollectionAsync();
        Task AutoCheckCollectionAsync();
        Task AutoCheckCollectionIfFoldersChangedAsync();
        Task QuickCheckCollectionAsync();
        event EventHandler IndexingStarted;
        event EventHandler IndexingStopped;
        event Action<IndexingStatusEventArgs> IndexingStatusChanged;
        event EventHandler RefreshLists;
        event EventHandler RefreshArtwork;
    }
}
