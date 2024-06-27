using Dopamine.Core.IO;
using NUnit.Framework;

namespace Dopamine.Tests
{
    [TestFixture]
    public class PlaylistDecoderTests
    {
        [Test]
        public void DecodeM3uPlaylist()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoder\Test.m3u");

            // Act
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.That(result.Paths.Count, Is.EqualTo(3));
                Assert.That(result.Paths[0], Is.EqualTo(@"C:\Music\File1.mp3"));
                Assert.That(result.Paths[1], Is.EqualTo(@"C:\Music\File2.mp3"));
                Assert.That(result.Paths[2], Is.EqualTo(@"C:\Music\File3.mp3"));
                Assert.That(result.PlaylistName, Is.EqualTo("Test"));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void DecodeM3u8Playlist()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoder\Test.m3u8");

            // Act
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.That(result.Paths.Count, Is.EqualTo(3));
                Assert.That(result.Paths[0], Is.EqualTo(@"C:\Music\File1.mp3"));
                Assert.That(result.Paths[1], Is.EqualTo(@"C:\Music\File2.mp3"));
                Assert.That(result.Paths[2], Is.EqualTo(@"C:\Music\File3.mp3"));
                Assert.That(result.PlaylistName, Is.EqualTo("Test"));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void DecodeZplPlaylist()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoder\Test.zpl");

            // Act
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.That(result.Paths.Count, Is.EqualTo(3));
                Assert.That(result.Paths[0], Is.EqualTo(@"C:\Music\File1.mp3"));
                Assert.That(result.Paths[1], Is.EqualTo(@"C:\Music\File2.mp3"));
                Assert.That(result.Paths[2], Is.EqualTo(@"C:\Music\File3.mp3"));
                Assert.That(result.PlaylistName, Is.EqualTo("ZPL test"));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void DecodeWplPlaylist()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoder\Test.wpl");

            // Act
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.That(result.Paths.Count, Is.EqualTo(3));
                Assert.That(result.Paths[0], Is.EqualTo(@"C:\Music\File1.mp3"));
                Assert.That(result.Paths[1], Is.EqualTo(@"C:\Music\File2.mp3"));
                Assert.That(result.Paths[2], Is.EqualTo(@"C:\Music\File3.mp3"));
                Assert.That(result.PlaylistName, Is.EqualTo("WPL test"));
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}