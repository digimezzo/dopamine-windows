using Dopamine.Core.Api.LyricWikia;
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
            string lyrics = await LyricWikiaApi.GetLyricsAsync("Massive Attack", "Teardrop");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod()]
        public async Task GetMalformedLyricsTest()
        {
            string lyrics = await LyricWikiaApi.GetLyricsAsync("masSivE AtTack", "tEarDrop");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }

        [TestMethod()]
        public async Task GetRedirectedLyricsTest()
        {
            string lyrics = await LyricWikiaApi.GetLyricsAsync("30 Seconds To Mars", "Echelon");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }
    }
}
