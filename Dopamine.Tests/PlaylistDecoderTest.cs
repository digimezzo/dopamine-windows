using Dopamine.Core.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dopamine.Tests
{
    [TestClass]
    public class PlaylistDecoderTest
    {
        [TestMethod(), TestCategory(TestCategories.PlaylistDecoder)]
        public void DecodeM3uPlaylist()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoder\Test.m3u");
            string playlistDirectory = System.IO.Path.GetDirectoryName(playlistPath);

            // Act
            var decoder = new Core.IO.PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && result.Paths.Count == 3 && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.IsTrue(result.Paths.Count == 3);
                Assert.AreEqual(result.Paths[0], @"C:\Music\File1.mp3");
                Assert.AreEqual(result.Paths[1], @"C:\Music\File2.mp3");
                Assert.AreEqual(result.Paths[2], @"C:\Music\File3.mp3");
                Assert.AreEqual(result.PlaylistName, "Test");
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod(), TestCategory(TestCategories.PlaylistDecoder)]
        public void DecodeZplPlaylist()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoder\Test.zpl");
            string playlistDirectory = System.IO.Path.GetDirectoryName(playlistPath);

            // Act
            var decoder = new Core.IO.PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && result.Paths.Count == 3 && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.IsTrue(result.Paths.Count == 3);
                Assert.AreEqual(result.Paths[0], @"C:\Music\File1.mp3");
                Assert.AreEqual(result.Paths[1], @"C:\Music\File2.mp3");
                Assert.AreEqual(result.Paths[2], @"C:\Music\File3.mp3");
                Assert.AreEqual(result.PlaylistName, "ZPL test");
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod(), TestCategory(TestCategories.PlaylistDecoder)]
        public void DecodeWplPlaylist()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoder\Test.wpl");
            string playlistDirectory = System.IO.Path.GetDirectoryName(playlistPath);

            // Act
            var decoder = new Core.IO.PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && result.Paths.Count == 3 && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.IsTrue(result.Paths.Count == 3);
                Assert.AreEqual(result.Paths[0], @"C:\Music\File1.mp3");
                Assert.AreEqual(result.Paths[1], @"C:\Music\File2.mp3");
                Assert.AreEqual(result.Paths[2], @"C:\Music\File3.mp3");
                Assert.AreEqual(result.PlaylistName, "WPL test");
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod(), TestCategory(TestCategories.PlaylistDecoder)]
        public void GenerateFullPath()
        {
            // Arrange
            var decoder = new PlaylistDecoderTest();
            PrivateObject obj = new PrivateObject(decoder);

            string expectedPath1 = @"C:\Music\Folder\File.mp3";
            string expectedPath2 = @"\\Device\Folder\Subfolder\File.mp3";
            string expectedPath3 = @"C:\Music\File.mp3";
            string expectedPath4 = @"C:\Music\File.mp3";
            string expectedPath5 = @"C:\Music\File.mp3";
            string expectedPath6 = @"C:\Music\Folder\File.mp3";
            string expectedPath7 = @"C:\Music\Folder\File.mp3";
            string expectedPath8 = @"C:\Music\Folder\File.mp3";
            string expectedPath9 = @"C:\Music\File.mp3";
            string expectedPath10 = @"C:\Music\Folder2\File.mp3";
            string expectedPath11 = @"C:\Music\Folder2\Folder3\File.mp3";
            string expectedPath12 = @"C:\Music\Folder2\File.mp3";

            // Act
            var generatedPath1 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @"C:\Music\Folder\File.mp3" });
            var generatedPath2 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @"\\Device\Folder\Subfolder\File.mp3" });
            var generatedPath3 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @"File.mp3" });
            var generatedPath4 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @"\File.mp3" });
            var generatedPath5 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @".\File.mp3" });
            var generatedPath6 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @"Folder\File.mp3" });
            var generatedPath7 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @"\Folder\File.mp3" });
            var generatedPath8 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Playlist.m3u", @".\Folder\File.mp3" });
            var generatedPath9 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Folder\Playlist.m3u", @"..\File.mp3" });
            var generatedPath10 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Folder1\Playlist.m3u", @"..\Folder2\File.mp3" });
            var generatedPath11 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Folder1\Playlist.m3u", @"..\Folder2\Folder3\File.mp3" });
            var generatedPath12 = obj.Invoke("GenerateFullTrackPath", new object[] { @"C:\Music\Folder1\Playlist.m3u", @".\..\Folder2\File.mp3" });

            // Assert
            Assert.AreEqual(expectedPath1, generatedPath1.ToString());
            Assert.AreEqual(expectedPath2, generatedPath2.ToString());
            Assert.AreEqual(expectedPath3, generatedPath3.ToString());
            Assert.AreEqual(expectedPath4, generatedPath4.ToString());
            Assert.AreEqual(expectedPath5, generatedPath5.ToString());
            Assert.AreEqual(expectedPath6, generatedPath6.ToString());
            Assert.AreEqual(expectedPath7, generatedPath7.ToString());
            Assert.AreEqual(expectedPath8, generatedPath8.ToString());
            Assert.AreEqual(expectedPath9, generatedPath9.ToString());
            Assert.AreEqual(expectedPath10, generatedPath10.ToString());
            Assert.AreEqual(expectedPath11, generatedPath11.ToString());
            Assert.AreEqual(expectedPath12, generatedPath12.ToString());
        }
    }
}
