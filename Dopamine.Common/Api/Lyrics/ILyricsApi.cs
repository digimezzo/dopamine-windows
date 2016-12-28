using System.Threading.Tasks;

namespace Dopamine.Common.Api.Lyrics
{
    public interface ILyricsApi
    {
        string SourceName { get; }
        Task<string> GetLyricsAsync(string artist, string title);
    }
}