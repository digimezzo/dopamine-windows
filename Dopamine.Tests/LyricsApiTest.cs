using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Dopamine.Tests
{
    [TestClass]
    public class LyricsApiTest
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

        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetSpecialCharactersLyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("Therapy?", "Loose");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetSpecialCharacters2Lyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("Skinny Puppy", "God's Gift (Maggot)");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetSpecialCasingLyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("KMFDM", "Splatter");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetSpecialCasing2Lyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("Out Out", "S.Y.O.");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod(), TestCategory(TestCategories.LyricsApi)]
        public async Task LyricWikiaApiGetSpecialCasing3Lyrics()
        {
            var api = new Core.Api.Lyrics.LyricWikiaApi(0);
            string lyrics = await api.GetLyricsAsync("BiGod 20", "The Big Bang");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }
    }
}
