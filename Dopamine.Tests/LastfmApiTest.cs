using Dopamine.Core.API.Lastfm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Dopamine.Tests
{
    [TestClass]
    public class LastfmAPITest
    {
        [TestMethod]
        public async Task AuthenticationTestASync()
        {
            var lastfmAPI = new LastfmAPI();

            var session = await lastfmAPI.GetMobileSession("digimezzo", "$$trustno1");
        }
    }
}
