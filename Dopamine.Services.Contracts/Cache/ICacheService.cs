using System;
using System.Threading.Tasks;

namespace Dopamine.Services.Contracts.Cache
{
    public interface ICacheService
    {
        string CoverArtCacheFolderPath { get; }
        string TemporaryCacheFolderPath { get; }
        Task<string> CacheArtworkAsync(byte[] artwork);
        Task<string> CacheArtworkAsync(Uri uri);
        string GetCachedArtworkPath(string artworkID);
        Task<string> DownloadFileToTemporaryCacheAsync(Uri uri);
    }
}
