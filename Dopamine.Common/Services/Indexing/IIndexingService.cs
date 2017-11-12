using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Indexing
{
    public delegate void AlbumArtworkAddedEventHandler(object sender, AlbumArtworkAddedEventArgs e);

    public interface IIndexingService
    {
        void OnFoldersChanged();
        bool IsIndexing { get; }
        Task RefreshCollectionAsync();
        Task RefreshCollectionIfFoldersChangedAsync();
        Task RefreshCollectionImmediatelyAsync();
        event EventHandler IndexingStarted;
        event EventHandler IndexingStopped;
        event Action<IndexingStatusEventArgs> IndexingStatusChanged;
        event EventHandler RefreshLists;
        event AlbumArtworkAddedEventHandler AlbumArtworkAdded;
    }
}
