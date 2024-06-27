using Dopamine.Core.Api.Lyrics;
using Dopamine.Services.Lyrics;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Dopamine.Tests
{
    [TestFixture]
    public class LyricsParserTests
    {
        private readonly ILyricsService service = new LyricsService();

        [Test]
        public void ParseMixedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\MixedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.That(lyricsLines.Count, Is.EqualTo(9));
            Assert.That(lyricsLines[0].Text, Is.EqualTo("Synchronized line 1"));
            Assert.That(lyricsLines[1].Text, Is.EqualTo("Repeating synchronized line 1"));
            Assert.That(lyricsLines[2].Text, Is.EqualTo("Synchronized line 2"));
            Assert.That(lyricsLines[3].Text, Is.EqualTo("Repeating synchronized line 1"));
            Assert.That(lyricsLines[4].Text, Is.EqualTo("[Incorrect line 1]"));
            Assert.That(lyricsLines[5].Text, Is.EqualTo("Unsynchronized line 1"));
            Assert.That(lyricsLines[6].Text, Is.EqualTo("[00:17.20][00:ss.20]Incorrect line 2"));
            Assert.That(lyricsLines[7].Text, Is.EqualTo("Unsynchronized line 2"));
            Assert.That(lyricsLines[8].Text, Is.EqualTo("Unsynchronized line 3"));
        }

        [Test]
        public void ParseUnsynchronizedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\UnsynchronizedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.That(lyricsLines.Count, Is.EqualTo(15));
            Assert.That(lyricsLines[0].Text, Is.EqualTo("Unsynchronized line 1"));
            Assert.That(lyricsLines[1].Text, Is.EqualTo("Unsynchronized line 2"));
            Assert.That(lyricsLines[2].Text, Is.EqualTo("Unsynchronized line 3"));
            Assert.That(lyricsLines[3].Text, Is.EqualTo("Unsynchronized line 4"));
            Assert.That(lyricsLines[4].Text, Is.EqualTo("Unsynchronized line 5"));
            Assert.That(lyricsLines[5].Text, Is.EqualTo("Unsynchronized line 6"));
            Assert.That(lyricsLines[6].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[7].Text, Is.EqualTo("Unsynchronized line 7"));
            Assert.That(lyricsLines[8].Text, Is.EqualTo("Unsynchronized line 8"));
            Assert.That(lyricsLines[9].Text, Is.EqualTo("Unsynchronized line 9"));
            Assert.That(lyricsLines[10].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[11].Text, Is.EqualTo("Unsynchronized line 10"));
            Assert.That(lyricsLines[12].Text, Is.EqualTo("Unsynchronized line 11"));
            Assert.That(lyricsLines[13].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[14].Text, Is.EqualTo("Unsynchronized line 12"));
        }

        [Test]
        public void ParseSynchronizedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\SynchronizedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.That(lyricsLines.Count, Is.EqualTo(15));
            Assert.That(lyricsLines[0].Text, Is.EqualTo("Synchronized line 1"));
            Assert.That(lyricsLines[1].Text, Is.EqualTo("Synchronized line 2"));
            Assert.That(lyricsLines[2].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[3].Text, Is.EqualTo("Synchronized line 3"));
            Assert.That(lyricsLines[4].Text, Is.EqualTo("Synchronized line 4"));
            Assert.That(lyricsLines[5].Text, Is.EqualTo("Synchronized line 5"));
            Assert.That(lyricsLines[6].Text, Is.EqualTo("Synchronized line 6"));
            Assert.That(lyricsLines[7].Text, Is.EqualTo("Synchronized line 7"));
            Assert.That(lyricsLines[8].Text, Is.EqualTo("Synchronized line 8"));
            Assert.That(lyricsLines[9].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[10].Text, Is.EqualTo("Synchronized line 9"));
            Assert.That(lyricsLines[11].Text, Is.EqualTo("Synchronized line 10"));
            Assert.That(lyricsLines[12].Text, Is.EqualTo("Synchronized line 11"));
            Assert.That(lyricsLines[13].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[14].Text, Is.EqualTo("Synchronized line 12"));
        }

        [Test]
        public void ParseSynchronizedLyricsWithDuplicateTimestampsKeepsOriginalOrder()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\SynchronizedLyricsWithDuplicateTimestamps.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.That(lyricsLines.Count, Is.EqualTo(12));
            Assert.That(lyricsLines[0].Text, Is.EqualTo("Synchronized line 1"));
            Assert.That(lyricsLines[1].Text, Is.EqualTo("Synchronized line 2"));
            Assert.That(lyricsLines[2].Text, Is.EqualTo("Synchronized line 3"));
            Assert.That(lyricsLines[3].Text, Is.EqualTo("Synchronized line 4"));
            Assert.That(lyricsLines[4].Text, Is.EqualTo("Synchronized line 5"));
            Assert.That(lyricsLines[5].Text, Is.EqualTo("Synchronized line 6"));
            Assert.That(lyricsLines[6].Text, Is.EqualTo("Synchronized line 7"));
            Assert.That(lyricsLines[7].Text, Is.EqualTo("Synchronized line 8"));
            Assert.That(lyricsLines[8].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[9].Text, Is.EqualTo("Synchronized line 10"));
            Assert.That(lyricsLines[10].Text, Is.EqualTo("Synchronized line 11"));
            Assert.That(lyricsLines[11].Text, Is.EqualTo("Synchronized line 12"));
        }

        [Test]
        public void ParseExtendedSynchronizedLyrics()
        {
            string lyricsText = File.ReadAllText(@"Files\LyricsParser\ExtendedSynchronizedLyrics.lrc");
            var lyrics = new Lyrics(lyricsText, "Not important");
            IList<LyricsLineViewModel> lyricsLines = this.service.ParseLyrics(lyrics);

            Assert.That(lyricsLines.Count, Is.EqualTo(24));
            Assert.That(lyricsLines[0].Text, Is.EqualTo("Synchronized line 1"));
            Assert.That(lyricsLines[1].Text, Is.EqualTo("Synchronized line 2"));
            Assert.That(lyricsLines[2].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[3].Text, Is.EqualTo("Synchronized line 3"));
            Assert.That(lyricsLines[4].Text, Is.EqualTo("Synchronized line 4"));
            Assert.That(lyricsLines[5].Text, Is.EqualTo("Synchronized line 5"));
            Assert.That(lyricsLines[6].Text, Is.EqualTo("Synchronized line 6"));
            Assert.That(lyricsLines[7].Text, Is.EqualTo("Repeating synchronized line 1"));
            Assert.That(lyricsLines[8].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[9].Text, Is.EqualTo("Synchronized line 7"));
            Assert.That(lyricsLines[10].Text, Is.EqualTo("Repeating synchronized line 2"));
            Assert.That(lyricsLines[11].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[12].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[13].Text, Is.EqualTo("Repeating synchronized line 1"));
            Assert.That(lyricsLines[14].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[15].Text, Is.EqualTo("Synchronized line 8"));
            Assert.That(lyricsLines[16].Text, Is.EqualTo("Synchronized line 9"));
            Assert.That(lyricsLines[17].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[18].Text, Is.EqualTo("Repeating synchronized line 2"));
            Assert.That(lyricsLines[19].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[20].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[21].Text, Is.EqualTo("Repeating synchronized line 1"));
            Assert.That(lyricsLines[22].Text, Is.EqualTo(""));
            Assert.That(lyricsLines[23].Text, Is.EqualTo("Synchronized line 10"));
        }
    }
}