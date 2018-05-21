using Dopamine.Data;
using Dopamine.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Collection
{
    public interface ICollectionService
    {
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks);
        Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks);
        Task MarkFolderAsync(Folder folder);
        event EventHandler CollectionChanged;
    }
}
