using Dopamine.Data.Entities;
using Dopamine.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Contracts.Metadata
{
    public interface IMetadataService
    {
        bool IsUpdatingDatabaseMetadata { get; }

        Task UpdateTrackRatingAsync(string path, int rating);

        Task UpdateTrackLoveAsync(string path, bool love);

        Task UpdateTracksAsync(List<IFileMetadata> fileMetadatas, bool updateAlbumArtwork);

        Task UpdateAlbumAsync(Album album, MetadataArtworkValue artwork, bool updateFileArtwork);

        IFileMetadata GetFileMetadata(string path);

        Task<IFileMetadata> GetFileMetadataAsync(string path);

        Task<byte[]> GetArtworkAsync(string path, int size = 0);
        
        Task SafeUpdateFileMetadataAsync();

        event Action<MetadataChangedEventArgs> MetadataChanged;
        event Action<RatingChangedEventArgs> RatingChanged;
        event Action<LoveChangedEventArgs> LoveChanged;
    }
}
