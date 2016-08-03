using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dopamine.Core.IO;

namespace Dopamine.Tests
{
    [TestClass]
    public class PlaylistDecoderTest
    {
        [TestMethod]
        public void DecodeM3uPlaylistTest()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath( @"Files\PlaylistDecoderTest\Directory1\Test.m3u");
            string playlistDirectory = System.IO.Path.GetDirectoryName(playlistPath);

            // Act
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && result.Paths.Count == 11 && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.IsTrue(result.Paths.Count == 11);
                Assert.AreEqual(result.Paths[0], "C:\\Users\\Digimezzo\\Desktop\\Dummy.mp3");
                Assert.IsTrue(System.IO.File.Exists(result.Paths[1]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[2]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[3]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[4]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[5]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[6]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[7]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[8]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[9]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[10]));
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void DecodeZplPlaylistTest()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoderTest\Directory1\Test.zpl");
            string playlistDirectory = System.IO.Path.GetDirectoryName(playlistPath);

            // Act
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && result.Paths.Count == 11 && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.IsTrue(result.Paths.Count == 11);
                Assert.AreEqual(result.Paths[0], "C:\\Users\\Digimezzo\\Desktop\\Dummy.mp3");
                Assert.IsTrue(System.IO.File.Exists(result.Paths[1]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[2]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[3]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[4]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[5]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[6]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[7]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[8]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[9]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[10]));
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void DecodeWplPlaylistTest()
        {
            // Arrange
            string playlistPath = System.IO.Path.GetFullPath(@"Files\PlaylistDecoderTest\Directory1\Test.wpl");
            string playlistDirectory = System.IO.Path.GetDirectoryName(playlistPath);

            // Act
            var decoder = new PlaylistDecoder();
            DecodePlaylistResult result = decoder.DecodePlaylist(playlistPath);

            // Assert
            if (result.DecodeResult.Result && result.Paths.Count == 11 && !string.IsNullOrWhiteSpace(result.PlaylistName))
            {
                Assert.IsTrue(result.Paths.Count == 11);
                Assert.AreEqual(result.Paths[0], "C:\\Users\\Digimezzo\\Desktop\\Dummy.mp3");
                Assert.IsTrue(System.IO.File.Exists(result.Paths[1]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[2]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[3]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[4]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[5]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[6]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[7]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[8]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[9]));
                Assert.IsTrue(System.IO.File.Exists(result.Paths[10]));
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
