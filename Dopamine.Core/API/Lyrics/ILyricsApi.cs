using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    public interface ILyricsApi
    {
        Task<string> GetLyricsAsync(string artist, string title);
    }
}