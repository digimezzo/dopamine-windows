using System.Collections.Generic;

namespace Dopamine.Services.Lyrics
{
    public interface ILyricsService
    {
        IList<LyricsLineViewModel> ParseLyrics(Core.Api.Lyrics.Lyrics lyrics);
    }
}
