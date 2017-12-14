using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Dopamine.Tests
{
    [TestClass]
    public class LyricsApi
    {
        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetNormalLyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("Massive Attack", "Teardrop");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetMalformedLyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("masSivE AtTack", "tEarDrop");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetRedirectedLyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("30 Seconds To Mars", "Echelon");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }
    }
}
