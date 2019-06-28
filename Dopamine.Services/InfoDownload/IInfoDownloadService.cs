using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.InfoDownload
{
    public interface IInfoDownloadService
    {
        Task<string> GetAlbumImageAsync(string albumTitle, IList<string> albumArtists, string trackTitle = "", IList<string> trackArtists = null);
    }
}
