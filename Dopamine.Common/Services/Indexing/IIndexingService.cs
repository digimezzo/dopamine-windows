using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Indexing
{
    public interface IIndexingService
    {
        bool IsIndexing { get; }
        bool NeedsIndexing { get; set; }
        Task CheckCollectionAsync(bool ignoreRemovedFiles, bool artworkOnly);
        Task DelayedIndexCollectionAsync(int delayMilliseconds, bool ignoreRemovedFiles, bool artworkOnly, bool isInitialized = false);
        Task IndexCollectionAsync(bool ignoreRemovedFiles, bool artworkOnly, bool isInitialized = false);
        event EventHandler IndexingStarted;
        event EventHandler IndexingStopped;
        event Action<IndexingStatusEventArgs> IndexingStatusChanged;
        event EventHandler RefreshLists;
        event EventHandler RefreshArtwork;
        void RefreshNow();
    }
}
