using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Cache
{
    public interface ICacheService
    {
        string CoverArtCacheFolderPath { get; }
        string TemporaryCacheFolderPath { get; }
        Task<string> CacheArtworkAsync(byte[] artwork);
        string GetCachedArtworkPath(string artworkID);
        Task<string> DownloadFileToTemporaryCacheAsync(Uri uri);
    }
}
