using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Contracts.Collection
{
    public interface ICollectionService
    {
        Task<RemoveTracksResult> RemoveTracksFromCollectionAsync(IList<PlayableTrack> selectedTracks);
        Task<RemoveTracksResult> RemoveTracksFromDiskAsync(IList<PlayableTrack> selectedTracks);
        Task MarkFolderAsync(Folder folder);
        event EventHandler CollectionChanged;
    }
}
