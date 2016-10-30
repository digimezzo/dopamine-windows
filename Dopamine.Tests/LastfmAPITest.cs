using Dopamine.Core.API.Lastfm;
using Dopamine.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Dopamine.Tests
{
    [TestClass]
    public class LastfmAPITest
    {
        //private string username = "<username>";
        //private string password = "<password>";
        private string username = "digimezzo";
        private string password = "$$trustno1";

        [TestMethod()]
        public async Task AuthenticationTest()
        {
            string sessionKey = await LastfmAPI.GetMobileSession(this.username, this.password);

            Assert.IsTrue(!string.IsNullOrEmpty(sessionKey));
        }

        [TestMethod()]
        public async Task UpdateNowPlayingTest()
        {
            string sessionKey = await LastfmAPI.GetMobileSession(this.username, this.password);
            bool isSuccess = await LastfmAPI.TrackUpdateNowPlaying(sessionKey, "Jetta", "I'd Love to Change the World", "");

            Assert.IsTrue(isSuccess);
        }

        [TestMethod()]
        public async Task ScrobbleTest()
        {
            string sessionKey = await LastfmAPI.GetMobileSession(this.username, this.password);
            bool isSuccess = await LastfmAPI.TrackScrobble(sessionKey, "Coldplay", "Viva La Vida", "", DateTime.Now);

            Assert.IsTrue(isSuccess);
       } 

        [TestMethod()]
        public async Task ArtistGetInfoTest()
        {
            LastFmArtist lfmArtist = await LastfmAPI.ArtistGetInfo("Coldplay",false, string.Empty);

            Assert.IsTrue(!string.IsNullOrEmpty(lfmArtist.Name) & !string.IsNullOrEmpty(lfmArtist.Url));
        }

        [TestMethod()]
        public async Task AlbumGetInfoTest()
        {
            LastFmAlbum lfmAlbum = await LastfmAPI.AlbumGetInfo("Coldplay", "Viva la Vida or Death and All His Friends",false, string.Empty);

            Assert.IsTrue(!string.IsNullOrEmpty(lfmAlbum.Name) & !string.IsNullOrEmpty(lfmAlbum.Url));
        }

        [TestMethod()]
        public async Task TrackLoveTest()
        {
            string sessionKey = await LastfmAPI.GetMobileSession(this.username, this.password);
            bool isSuccess = await LastfmAPI.TrackLove(sessionKey, "Madonna", "Like a Virgin");

            Assert.IsTrue(isSuccess);
        }

        [TestMethod()]
        public async Task TrackUnloveTest()
        {
            string sessionKey = await LastfmAPI.GetMobileSession(this.username, this.password);
            bool isSuccess = await LastfmAPI.TrackUnlove(sessionKey, "Coldplay", "Viva La Vida");

            Assert.IsTrue(isSuccess);
        }
    }
}
