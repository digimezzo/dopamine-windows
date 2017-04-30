using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dopamine.Common.Audio;

namespace Dopamine.Tests
{
    [TestClass]
    public class AudioFormatsMediaFoundation
    {
        #region Private
        private int PlayFile(string audioFile, bool wasapiEventMode, bool wasapiExclusiveMode, float volume, int skipSeconds = 0)
        {
            IPlayer player = CSCorePlayer.Instance;
            player.SetPlaybackSettings(200, wasapiEventMode, wasapiExclusiveMode, new EqualizerPreset().Bands);
            player.SetVolume(volume);
            player.Play(audioFile);
            if (skipSeconds > 0) player.Skip(skipSeconds);

            return Convert.ToInt32(player.GetCurrentTime().TotalSeconds);
        }
        #endregion

        #region MP3
        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void Mp3Basic()
        {
            this.PlayFile(@"Files\AudioFormats\test.mp3", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void Mp3Listen()
        {
            this.PlayFile(@"Files\AudioFormats\test.mp3", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void Mp3Skip()
        {
            int skipSeconds = 60;
            int currentSeconds = this.PlayFile(@"Files\AudioFormats\test.mp3", false, false, 0.0f, skipSeconds);
            Assert.IsTrue(currentSeconds == skipSeconds);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void Mp3Basic2()
        {
            this.PlayFile(@"Files\AudioFormats\test2.mp3", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void Mp3Listen2()
        {
            this.PlayFile(@"Files\AudioFormats\test2.mp3", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void Mp3Basic3()
        {
            this.PlayFile(@"Files\AudioFormats\test3.mp3", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void Mp3Listen3()
        {
            this.PlayFile(@"Files\AudioFormats\test3.mp3", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        // Mp3BasicTest4 and Mp3ListenTest4 fail, because that MP3 is not supported by Media Foundation.
        // As we use MediaFoundationDecoder to decode MP3's, this file is not supported in Dopamine.
        // DmoMp3Decoder can decode this file. However, DmoMp3Decoder is slow to play MP3's from a NAS. 
        // Once the NAS bug is fixed, we can use DmoMp3Decoder and try again to decode this MP3 file.

        //[TestMethod()]
        //public void Mp3Basic4()
        //{
        //    this.PlayFile(@"Files\AudioFormats\test4.mp3", false, false, 0.0f);
        //}

        //[TestMethod()]
        //public void Mp3Listen4()
        //{
        //    this.PlayFile(@"Files\AudioFormats\test4.mp3", false, false, 0.3f);
        //    System.Threading.Thread.Sleep(5000);
        //}
        #endregion

        #region WMA
        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void WmaBasic()
        {
            this.PlayFile(@"Files\AudioFormats\test.wma", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void WmaListen()
        {
            this.PlayFile(@"Files\AudioFormats\test.wma", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void WmaSkip()
        {
            int skipSeconds = 60;
            int currentSeconds = this.PlayFile(@"Files\AudioFormats\test.wma", false, false, 0.0f, skipSeconds);
            Assert.IsTrue(currentSeconds == skipSeconds);
        }
        #endregion

        #region FLAC
        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void FlacBasic()
        {
            this.PlayFile(@"Files\AudioFormats\test.flac", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void FlacListen()
        {
            this.PlayFile(@"Files\AudioFormats\test.flac", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void FlacSkip()
        {
            int skipSeconds = 60;
            int currentSeconds = this.PlayFile(@"Files\AudioFormats\test.flac", false, false, 0.0f, skipSeconds);
            Assert.IsTrue(currentSeconds == skipSeconds);
        }
        #endregion

        #region OGG
        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void OggBasic()
        {
            this.PlayFile(@"Files\AudioFormats\test.ogg", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void OggListen()
        {
            this.PlayFile(@"Files\AudioFormats\test.ogg", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void OggSkip()
        {
            int skipSeconds = 60;
            int currentSeconds = this.PlayFile(@"Files\AudioFormats\test.ogg", false, false, 0.0f, skipSeconds);
            Assert.IsTrue(currentSeconds == skipSeconds);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void OggBasic2()
        {
            // This file causes a System.IO.InvalidDataException: found invalid data while decoding. 
            // This was fixed by ioctlLR on 19-03-2015. This test is to make sure the issue doesn't come back.
            this.PlayFile(@"Files\AudioFormats\test2.ogg", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void OggListen2()
        {
            // This file causes a System.IO.InvalidDataException: found invalid data while decoding. 
            // This was fixed by ioctlLR on 19-03-2015. This test is to make sure the issue doesn't come back.
            this.PlayFile(@"Files\AudioFormats\test2.ogg", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void OggBasic3()
        {
            // This file doesn't follow the ogg spec. The encoder set the "Continuation" flag on the first non-header page, which is invalid.
            // Changing that byte (offset 79190) to "None" (0) fixes the decoding problem.
            // ioctlLR provided a workaround on 22-04-2015. He didn't yet modify his code at that time: I did it myself.
            // He advised me to comment out line 133 in OggPacketReader.cs: "if (!_last.IsContinued) throw new InvalidDataException();"
            // This test is to make sure the issue doesn't come back.
            this.PlayFile(@"Files\AudioFormats\test3.ogg", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void OggListen3()
        {
            // This file doesn't follow the ogg spec. The encoder set the "Continuation" flag on the first non-header page, which is invalid.
            // Changing that byte (offset 79190) to "None" (0) fixes the decoding problem.
            // ioctlLR provided a workaround on 22-04-2015. He didn't yet modify his code at that time: I did it myself.
            // He advised me to comment out line 133 in OggPacketReader.cs: "if (!_last.IsContinued) throw new InvalidDataException();"
            // This test is to make sure the issue doesn't come back.
            this.PlayFile(@"Files\AudioFormats\test3.ogg", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }
        #endregion

        #region M4A
        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void M4aBasicTest()
        {
            this.PlayFile(@"Files\AudioFormats\test.m4a", false, false, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void M4aListenTest()
        {
            this.PlayFile(@"Files\AudioFormats\test.m4a", false, false, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void M4aSkipTest()
        {
            int skipSeconds = 60;
            int currentSeconds = this.PlayFile(@"Files\AudioFormats\test.m4a", false, false, 0.0f, skipSeconds);
            Assert.IsTrue(currentSeconds == skipSeconds);
        }
        #endregion

        #region WASAPI Exclusive Mode
        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void WasapiExclusiveModeBasic()
        {
            this.PlayFile(@"Files\AudioFormats\test.mp3", false, true, 0.0f);
        }

        [TestMethod(), TestCategory(TestCategories.AudioFormatsMediaFoundation)]
        public void WasapiExclusiveModeListen()
        {
            this.PlayFile(@"Files\AudioFormats\test.mp3", false, true, 0.3f);
            System.Threading.Thread.Sleep(5000);
        }
        #endregion
    }
}
