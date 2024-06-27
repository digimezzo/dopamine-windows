using Dopamine.Core.Api.Lastfm;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dopamine.Tests
{
    [TestFixture]
    public class LastfmApiTests
    {
        private readonly string username = "<username>";
        private readonly string password = "<password>";

        [Test]
        public async Task AuthenticationTest()
        {
            string sessionKey = await Core.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);

            Assert.That(!string.IsNullOrEmpty(sessionKey), Is.True);
        }

        [Test]
        public async Task UpdateNowPlayingTest()
        {
            string sessionKey = await Core.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Core.Api.Lastfm.LastfmApi.TrackUpdateNowPlaying(sessionKey, "Jetta", "I'd Love to Change the World", "");

            Assert.That(isSuccess, Is.True);
        }

        [Test]
        public async Task ScrobbleTest()
        {
            string sessionKey = await Core.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Core.Api.Lastfm.LastfmApi.TrackScrobble(sessionKey, "Coldplay", "Viva La Vida", "", DateTime.Now);

            Assert.That(isSuccess, Is.True);
       }

        [Test]
        public async Task ArtistGetInfoTest()
        {
            LastFmArtist lfmArtist = await Core.Api.Lastfm.LastfmApi.ArtistGetInfo("Coldplay",false, string.Empty);

            Assert.That(!string.IsNullOrEmpty(lfmArtist.Name) & !string.IsNullOrEmpty(lfmArtist.Url), Is.True);
        }

        [Test]
        public async Task AlbumGetInfoTest()
        {
            LastFmAlbum lfmAlbum = await Core.Api.Lastfm.LastfmApi.AlbumGetInfo("Coldplay", "Viva la Vida or Death and All His Friends",false, string.Empty);

            Assert.That(!string.IsNullOrEmpty(lfmAlbum.Name) & !string.IsNullOrEmpty(lfmAlbum.Url), Is.True);
        }

        /// <summary>
        /// This test is a bit useless, as track.love always returns "ok", even if it failed.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TrackLoveTest()
        {
            string sessionKey = await Core.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Core.Api.Lastfm.LastfmApi.TrackLove(sessionKey, "Madonna", "Like a Virgin");

            Assert.That(isSuccess, Is.True);
        }

        /// <summary>
        /// This test is a bit useless, as track.unlove always returns "ok", even if it failed.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TrackUnloveTest()
        {
            string sessionKey = await Core.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Core.Api.Lastfm.LastfmApi.TrackUnlove(sessionKey, "Madonna", "Like a Virgin");

            Assert.That(isSuccess, Is.True);
        }
    }
}
