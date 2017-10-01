using Dopamine.Core.Services.Indexing;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Indexing
{
    public interface IIndexingService
    {
        bool IsIndexing { get; }
        Task CheckCollectionAsync();
        Task IndexCollectionAsync(bool artworkOnly = false);
        event EventHandler IndexingStarted;
        event EventHandler IndexingStopped;
        event Action<IndexingStatusEventArgs> IndexingStatusChanged;
        event EventHandler RefreshLists;
        event EventHandler RefreshArtwork;
        void OnFoldersChanged();
    }
}
