using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lyrics
{
    public interface ILyricsApi
    {
        string SourceName { get; }
        Task<string> GetLyricsAsync(string artist, string title);
    }
}