using Dopamine.Core.Api.Lyrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Dopamine.Tests
{
    [TestClass]
    public class LyricWikiaApiTest
    {
        [TestMethod()]
        public async Task GetNormalLyricsTest()
        {
            var api = new LyricWikiaApi();
            string lyrics = await api.GetLyricsAsync("Massive Attack", "Teardrop");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod()]
        public async Task GetMalformedLyricsTest()
        {
            var api = new LyricWikiaApi();
            string lyrics = await api.GetLyricsAsync("masSivE AtTack", "tEarDrop");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod()]
        public async Task GetRedirectedLyricsTest()
        {
            var api = new LyricWikiaApi();
            string lyrics = await api.GetLyricsAsync("30 Seconds To Mars", "Echelon");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }
    }
}
