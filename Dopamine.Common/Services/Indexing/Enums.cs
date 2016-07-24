namespace Dopamine.Common.Services.Indexing
{
    public enum IndexingAction
    {
        Idle = 1,
        RemoveTracks = 2,
        AddTracks = 3,
        UpdateTracks = 4,
        UpdateArtwork = 5
    }

    public enum IndexingDataChanged
    {
        None = 1,
        Tracks = 2,
        Artwork = 3
    }
}
