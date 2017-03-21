using Dopamine.Common.Api.Lastfm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Dopamine.Tests
{
    [TestClass]
    public class LastfmApi
    {
        private string username = "<username>";
        private string password = "<password>";

        [TestMethod(), TestCategory(TestCategories.LastfmApi)]
        public async Task AuthenticationTest()
        {
            string sessionKey = await Common.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);

            Assert.IsTrue(!string.IsNullOrEmpty(sessionKey));
        }

        [TestMethod(), TestCategory(TestCategories.LastfmApi)]
        public async Task UpdateNowPlayingTest()
        {
            string sessionKey = await Common.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Common.Api.Lastfm.LastfmApi.TrackUpdateNowPlaying(sessionKey, "Jetta", "I'd Love to Change the World", "");

            Assert.IsTrue(isSuccess);
        }

        [TestMethod(), TestCategory(TestCategories.LastfmApi)]
        public async Task ScrobbleTest()
        {
            string sessionKey = await Common.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Common.Api.Lastfm.LastfmApi.TrackScrobble(sessionKey, "Coldplay", "Viva La Vida", "", DateTime.Now);

            Assert.IsTrue(isSuccess);
       }

        [TestMethod(), TestCategory(TestCategories.LastfmApi)]
        public async Task ArtistGetInfoTest()
        {
            Artist lfmArtist = await Common.Api.Lastfm.LastfmApi.ArtistGetInfo("Coldplay",false, string.Empty);

            Assert.IsTrue(!string.IsNullOrEmpty(lfmArtist.Name) & !string.IsNullOrEmpty(lfmArtist.Url));
        }

        [TestMethod(), TestCategory(TestCategories.LastfmApi)]
        public async Task AlbumGetInfoTest()
        {
            Album lfmAlbum = await Common.Api.Lastfm.LastfmApi.AlbumGetInfo("Coldplay", "Viva la Vida or Death and All His Friends",false, string.Empty);

            Assert.IsTrue(!string.IsNullOrEmpty(lfmAlbum.Name) & !string.IsNullOrEmpty(lfmAlbum.Url));
        }

        /// <summary>
        /// This test is a bit useless, as track.love always returns "ok", even if it failed.
        /// </summary>
        /// <returns></returns>
        [TestMethod(), TestCategory(TestCategories.LastfmApi)]
        public async Task TrackLoveTest()
        {
            string sessionKey = await Common.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Common.Api.Lastfm.LastfmApi.TrackLove(sessionKey, "Madonna", "Like a Virgin");

            Assert.IsTrue(isSuccess);
        }

        /// <summary>
        /// This test is a bit useless, as track.unlove always returns "ok", even if it failed.
        /// </summary>
        /// <returns></returns>
        [TestMethod(), TestCategory(TestCategories.LastfmApi)]
        public async Task TrackUnloveTest()
        {
            string sessionKey = await Common.Api.Lastfm.LastfmApi.GetMobileSession(this.username, this.password);
            bool isSuccess = await Common.Api.Lastfm.LastfmApi.TrackUnlove(sessionKey, "Madonna", "Like a Virgin");

            Assert.IsTrue(isSuccess);
        }
    }
}
