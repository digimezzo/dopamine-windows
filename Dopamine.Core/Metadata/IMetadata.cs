using System;

namespace Dopamine.Core.Metadata
{
    public interface IMetadata
    {
        string FileName { get; }
        int BitRate { get; }
        int SampleRate { get; }
        TimeSpan Duration { get; }
        string Type { get; }
        string MimeType { get; }

        string Title { get; set; }
        string Album { get; set; }
        string[] AlbumArtists { get; set; }
        string[] Artists { get; set; }
        string[] Genres { get; set; }
        string Comment { get; set; }
        string Grouping { get; set; }
        string Year { get; set; }
        string TrackNumber { get; set; }
        string TrackCount { get; set; }
        string DiscNumber { get; set; }
        string DiscCount { get; set; }
        int Rating { get; set; }
        byte[] Artwork { get; set; }

        void Save();
    }
}
