using Dopamine.Core.IO;
using NUnit.Framework;

namespace Dopamine.Tests
{
    [TestFixture]
    public class TrackPathGeneratorTests
    {
        [Test]
        public void GenerateFullPath()
        {
            // Arrange
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
            var generatedPath1 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u", @"C:\Music\Folder\File.mp3");
            var generatedPath2 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u",
                    @"\\Device\Folder\Subfolder\File.mp3");
            var generatedPath3 = TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u", @"File.mp3");
            var generatedPath4 = TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u", @"\File.mp3");
            var generatedPath5 = TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u", @".\File.mp3");
            var generatedPath6 = TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u", @"Folder\File.mp3");
            var generatedPath7 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u", @"\Folder\File.mp3");
            var generatedPath8 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Playlist.m3u", @".\Folder\File.mp3");
            var generatedPath9 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Folder\Playlist.m3u", @"..\File.mp3");
            var generatedPath10 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Folder1\Playlist.m3u", @"..\Folder2\File.mp3");
            var generatedPath11 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Folder1\Playlist.m3u",
                    @"..\Folder2\Folder3\File.mp3");
            var generatedPath12 =
                TrackPathGenerator.GenerateFullTrackPath(@"C:\Music\Folder1\Playlist.m3u", @".\..\Folder2\File.mp3");

            // Assert
            Assert.That(expectedPath1, Is.EqualTo(generatedPath1.ToString()));
            Assert.That(expectedPath2, Is.EqualTo(generatedPath2.ToString()));
            Assert.That(expectedPath3, Is.EqualTo(generatedPath3.ToString()));
            Assert.That(expectedPath4, Is.EqualTo(generatedPath4.ToString()));
            Assert.That(expectedPath5, Is.EqualTo(generatedPath5.ToString()));
            Assert.That(expectedPath6, Is.EqualTo(generatedPath6.ToString()));
            Assert.That(expectedPath7, Is.EqualTo(generatedPath7.ToString()));
            Assert.That(expectedPath8, Is.EqualTo(generatedPath8.ToString()));
            Assert.That(expectedPath9, Is.EqualTo(generatedPath9.ToString()));
            Assert.That(expectedPath10, Is.EqualTo(generatedPath10.ToString()));
            Assert.That(expectedPath11, Is.EqualTo(generatedPath11.ToString()));
            Assert.That(expectedPath12, Is.EqualTo(generatedPath12.ToString()));
        }
    }
}