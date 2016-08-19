using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dopamine.Core.Audio;

namespace Dopamine.Tests
{
    [TestClass]
    public class AudioFormatsTest
    {
        [TestMethod()]
        public void Mp3BasicTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void Mp3ListenTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void Mp3SkipTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
            player.Skip(60);

            int currentSeconds = Convert.ToInt32(player.GetCurrentTime().TotalSeconds);

            Assert.IsTrue(currentSeconds == 60);
        }

        [TestMethod()]
        public void Mp3BasicTest2()
        {
            string audioFile = @"Files\AudioFormatsTest\test2.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void Mp3ListenTest2()
        {
            string audioFile = @"Files\AudioFormatsTest\test2.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void Mp3BasicTest3()
        {
            string audioFile = @"Files\AudioFormatsTest\test3.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void Mp3ListenTest3()
        {
            string audioFile = @"Files\AudioFormatsTest\test3.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void Mp3BasicTest4()
        {
            string audioFile = @"Files\AudioFormatsTest\test4.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void Mp3ListenTest4()
        {
            string audioFile = @"Files\AudioFormatsTest\test4.mp3";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void WmaBasicTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.wma";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void WmaListenTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.wma";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void WmaSkipTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.wma";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
            player.Skip(60);

            int currentSeconds = Convert.ToInt32(player.GetCurrentTime().TotalSeconds);

            Assert.IsTrue(currentSeconds == 60);
        }

        [TestMethod()]
        public void FlacBasicTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.flac";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void FlacListenTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.flac";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void FlacSkipTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.flac";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
            player.Skip(60);

            int currentSeconds = Convert.ToInt32(player.GetCurrentTime().TotalSeconds);

            Assert.IsTrue(currentSeconds == 60);
        }

        [TestMethod()]
        public void OggBasicTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.ogg";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void OggListenTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.ogg";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void OggSkipTest()
        {
            string audioFile = @"Files\AudioFormatsTest\test.ogg";

            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);

            player.Skip(60);

            int currentSeconds = Convert.ToInt32(player.GetCurrentTime().TotalSeconds);

            Assert.IsTrue(currentSeconds == 60);
        }

        [TestMethod()]
        public void OggBasicTest2()
        {
            string audioFile = @"Files\AudioFormatsTest\test2.ogg";

            // Issue with this file
            // --------------------
            // At some point it caused a System.IO.InvalidDataException: found invalid data while decoding. 
            // This was fixed by ioctlLR on 19-03-2015.
            // This test is to make sure the issue doesn't come back.
            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void OggListenTest2()
        {
            string audioFile = @"Files\AudioFormatsTest\test2.ogg";

            // Issue with this file
            // --------------------
            // At some point it caused a System.IO.InvalidDataException: found invalid data while decoding. 
            // This was fixed by ioctlLR on 19-03-2015.
            // This test is to make sure the issue doesn't come back.
            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod()]
        public void OggBasicTest3()
        {
            string audioFile = @"Files\AudioFormatsTest\test3.ogg";

            // Issue with this file
            // --------------------
            // This file doesn't follow the ogg spec. The encoder set the "Continuation" flag on the first non-header page, which is invalid.
            // Changing that byte (offset 79190) to "None" (0) fixes the decoding problem.
            // ioctlLR provided a workaround on 22-04-2015. He didn't yet modify his code at that time: I did it myself.
            // He advised me to comment out line 133 in OggPacketReader.cs: "if (!_last.IsContinued) throw new InvalidDataException();"
            // This test is to make sure the issue doesn't come back.
            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.0f);
            player.Play(audioFile);
        }

        [TestMethod()]
        public void OggListenTest3()
        {
            string audioFile = @"Files\AudioFormatsTest\test3.ogg";

            // Issue with this file
            // --------------------
            // This file doesn't follow the ogg spec. The encoder set the "Continuation" flag on the first non-header page, which is invalid.
            // Changing that byte (offset 79190) to "None" (0) fixes the decoding problem.
            // ioctlLR provided a workaround on 22-04-2015. He didn't yet modify his code at that time: I did it myself.
            // He advised me to comment out line 133 in OggPacketReader.cs: "if (!_last.IsContinued) throw new InvalidDataException();"
            // This test is to make sure the issue doesn't come back.
            IPlayer player = CSCorePlayer.Instance;
            player.SetOutputDevice(200, false, false, new EqualizerPreset("Dummy", false));
            player.SetVolume(0.3f);
            player.Play(audioFile);
            System.Threading.Thread.Sleep(5000);
        }
    }
}
