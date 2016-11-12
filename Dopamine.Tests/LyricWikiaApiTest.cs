using Dopamine.Core.Api.LyricWikia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Dopamine.Tests
{
    [TestClass]
    public class LyricWikiaApiTest
    {
        [TestMethod()]
        public async Task GetLyricsTest()
        {
            string lyrics = await LyricWikiaApi.GetLyricsAsync("masSivE AtTack", "tEarDrop");

            Assert.IsTrue(!string.IsNullOrEmpty(lyrics));
        }
    }
}
