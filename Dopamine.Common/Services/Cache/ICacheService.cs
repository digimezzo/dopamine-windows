namespace Dopamine.Common.Services.Cache
{
    public interface ICacheService
    {
        string CoverArtCacheFolderPath { get; }
        string TemporaryCacheFolderPath { get; }
        string CacheArtwork(byte[] artwork);
        string GetCachedArtworkPath(string artworkID);
    }
}
