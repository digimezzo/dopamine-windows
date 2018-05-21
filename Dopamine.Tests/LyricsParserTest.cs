using Dopamine.Core.Api.Lyrics;
using Dopamine.Services.Lyrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace Dopamine.Tests
{
    [TestClass]
    public class LyricsParserTest
    {
        private ILyricsService service = new LyricsService();

        [TestMethod(), TestCategory(TestCategories.LyricsParser)]
        public void ParseMixedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\MixedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.AreEqual(lyricsLines.Count, 9);
            Assert.AreEqual(lyricsLines[0].Text, "Synchronized line 1");
            Assert.AreEqual(lyricsLines[1].Text, "Repeating synchronized line 1");
            Assert.AreEqual(lyricsLines[2].Text, "Synchronized line 2");
            Assert.AreEqual(lyricsLines[3].Text, "Repeating synchronized line 1");
            Assert.AreEqual(lyricsLines[4].Text, "[Incorrect line 1]");
            Assert.AreEqual(lyricsLines[5].Text, "Unsynchronized line 1");
            Assert.AreEqual(lyricsLines[6].Text, "[00:17.20][00:ss.20]Incorrect line 2");
            Assert.AreEqual(lyricsLines[7].Text, "Unsynchronized line 2");
            Assert.AreEqual(lyricsLines[8].Text, "Unsynchronized line 3");
        }

        [TestMethod(), TestCategory(TestCategories.LyricsParser)]
        public void ParseUnsynchronizedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\UnsynchronizedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.AreEqual(lyricsLines.Count, 15);
            Assert.AreEqual(lyricsLines[0].Text, "Unsynchronized line 1");
            Assert.AreEqual(lyricsLines[1].Text, "Unsynchronized line 2");
            Assert.AreEqual(lyricsLines[2].Text, "Unsynchronized line 3");
            Assert.AreEqual(lyricsLines[3].Text, "Unsynchronized line 4");
            Assert.AreEqual(lyricsLines[4].Text, "Unsynchronized line 5");
            Assert.AreEqual(lyricsLines[5].Text, "Unsynchronized line 6");
            Assert.AreEqual(lyricsLines[6].Text, "");
            Assert.AreEqual(lyricsLines[7].Text, "Unsynchronized line 7");
            Assert.AreEqual(lyricsLines[8].Text, "Unsynchronized line 8");
            Assert.AreEqual(lyricsLines[9].Text, "Unsynchronized line 9");
            Assert.AreEqual(lyricsLines[10].Text, "");
            Assert.AreEqual(lyricsLines[11].Text, "Unsynchronized line 10");
            Assert.AreEqual(lyricsLines[12].Text, "Unsynchronized line 11");
            Assert.AreEqual(lyricsLines[13].Text, "");
            Assert.AreEqual(lyricsLines[14].Text, "Unsynchronized line 12");
        }

        [TestMethod(), TestCategory(TestCategories.LyricsParser)]
        public void ParseSynchronizedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\SynchronizedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.AreEqual(lyricsLines.Count, 15);
            Assert.AreEqual(lyricsLines[0].Text, "Synchronized line 1");
            Assert.AreEqual(lyricsLines[1].Text, "Synchronized line 2");
            Assert.AreEqual(lyricsLines[2].Text, "");
            Assert.AreEqual(lyricsLines[3].Text, "Synchronized line 3");
            Assert.AreEqual(lyricsLines[4].Text, "Synchronized line 4");
            Assert.AreEqual(lyricsLines[5].Text, "Synchronized line 5");
            Assert.AreEqual(lyricsLines[6].Text, "Synchronized line 6");
            Assert.AreEqual(lyricsLines[7].Text, "Synchronized line 7");
            Assert.AreEqual(lyricsLines[8].Text, "Synchronized line 8");
            Assert.AreEqual(lyricsLines[9].Text, "");
            Assert.AreEqual(lyricsLines[10].Text, "Synchronized line 9");
            Assert.AreEqual(lyricsLines[11].Text, "Synchronized line 10");
            Assert.AreEqual(lyricsLines[12].Text, "Synchronized line 11");
            Assert.AreEqual(lyricsLines[13].Text, "");
            Assert.AreEqual(lyricsLines[14].Text, "Synchronized line 12");
        }

        [TestMethod(), TestCategory(TestCategories.LyricsParser)]
        public void ParseExtendedSynchronizedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\ExtendedSynchronizedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.AreEqual(lyricsLines.Count, 24);
            Assert.AreEqual(lyricsLines[0].Text, "Synchronized line 1");
            Assert.AreEqual(lyricsLines[1].Text, "Synchronized line 2");
            Assert.AreEqual(lyricsLines[2].Text, "");
            Assert.AreEqual(lyricsLines[3].Text, "Synchronized line 3");
            Assert.AreEqual(lyricsLines[4].Text, "Synchronized line 4");
            Assert.AreEqual(lyricsLines[5].Text, "Synchronized line 5");
            Assert.AreEqual(lyricsLines[6].Text, "Synchronized line 6");
            Assert.AreEqual(lyricsLines[7].Text, "Repeating synchronized line 1");
            Assert.AreEqual(lyricsLines[8].Text, "");
            Assert.AreEqual(lyricsLines[9].Text, "Synchronized line 7");
            Assert.AreEqual(lyricsLines[10].Text, "Repeating synchronized line 2");
            Assert.AreEqual(lyricsLines[11].Text, "");
            Assert.AreEqual(lyricsLines[12].Text, "");
            Assert.AreEqual(lyricsLines[13].Text, "Repeating synchronized line 1");
            Assert.AreEqual(lyricsLines[14].Text, "");
            Assert.AreEqual(lyricsLines[15].Text, "Synchronized line 8");
            Assert.AreEqual(lyricsLines[16].Text, "Synchronized line 9");
            Assert.AreEqual(lyricsLines[17].Text, "");
            Assert.AreEqual(lyricsLines[18].Text, "Repeating synchronized line 2");
            Assert.AreEqual(lyricsLines[19].Text, "");
            Assert.AreEqual(lyricsLines[20].Text, "");
            Assert.AreEqual(lyricsLines[21].Text, "Repeating synchronized line 1");
            Assert.AreEqual(lyricsLines[22].Text, "");
            Assert.AreEqual(lyricsLines[23].Text, "Synchronized line 10");
        }
    }
}
