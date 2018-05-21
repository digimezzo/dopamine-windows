using System;

namespace Dopamine.Services.Contracts.Metadata
{
    public interface IFileMetadata
    {
        MetadataValue Album { get; set; }
        MetadataValue AlbumArtists { get; set; }
        MetadataValue Artists { get; set; }
        MetadataArtworkValue ArtworkData { get; set; }
        int BitRate { get; }
        MetadataValue Comment { get; set; }
        MetadataValue DiscCount { get; set; }
        MetadataValue DiscNumber { get; set; }
        TimeSpan Duration { get; }
        MetadataValue Genres { get; set; }
        MetadataValue Grouping { get; set; }
        MetadataValue Lyrics { get; set; }
        string MimeType { get; }
        string Path { get; }
        MetadataRatingValue Rating { get; set; }
        string SafePath { get; }
        int SampleRate { get; }
        MetadataValue Title { get; set; }
        MetadataValue TrackCount { get; set; }
        MetadataValue TrackNumber { get; set; }
        string Type { get; }
        MetadataValue Year { get; set; }

        bool Equals(object obj);
        int GetHashCode();
        void Save();
    }
}
