using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dopamine.Core.Audio;

namespace Dopamine.Tests
{
    [TestClass]
    public class WasapiTest
    {

        [TestMethod()]

        public void WasapiExclusiveModeBasicTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, true);
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]

        public void WasapiExclusiveModeListenTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, true);
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }
    }
}
