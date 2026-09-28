using Dopamine.Core.Base;
using NAudio.Flac;
using NAudio.MediaFoundation;
using NAudio.Vorbis;
using NAudio.Wave;
using NLayer.NAudioSupport;
using System;

namespace Dopamine.Core.Audio
{
    /// <summary>
    /// Opens audio files for decoding in layers:
    /// 1. Windows Media Foundation (fast, native, covers MP3/AAC/WMA/WAV and FLAC on Windows 10+).
    /// 2. Managed decoders (FLAC, MP3, OGG, WAV, AIFF).
    /// The managed decoders keep playback working on Windows versions which lack the Media
    /// Foundation codecs (for example the N/KN editions).
    /// </summary>
    public class NAudioDecoderFactory : IAudioDecoderFactory
    {
        private readonly bool hasMediaFoundationSupport;

        private static readonly object mediaFoundationLock = new object();
        private static bool mediaFoundationStarted;

        public NAudioDecoderFactory(bool hasMediaFoundationSupport)
        {
            this.hasMediaFoundationSupport = hasMediaFoundationSupport;
        }

        public WaveStream Open(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            WaveStream stream = this.TryOpenWithMediaFoundation(filename);

            if (stream == null)
            {
                stream = this.TryOpenManaged(filename);
            }

            if (stream == null)
            {
                throw new NotSupportedException($"No decoder is available for '{filename}'.");
            }

            return stream;
        }

        private WaveStream TryOpenWithMediaFoundation(string filename)
        {
            if (!this.hasMediaFoundationSupport)
            {
                return null;
            }

            try
            {
                StartMediaFoundation();

                return new MediaFoundationReader(filename);
            }
            catch (Exception)
            {
                // The file is not supported by the Media Foundation codecs on this machine.
                return null;
            }
        }

        private WaveStream TryOpenManaged(string filename)
        {
            string extension = System.IO.Path.GetExtension(filename).ToLowerInvariant();

            try
            {
                if (extension == FileFormats.FLAC)
                {
                    return new FlacReader(filename);
                }

                if (extension == FileFormats.MP3)
                {
                    // NLayer decodes MP3 without relying on the Windows ACM/DMO/MFT codecs.
                    Mp3FileReaderBase.FrameDecompressorBuilder frameDecompressorBuilder =
                        format => new Mp3FrameDecompressor(format);

                    return new Mp3FileReaderBase(filename, frameDecompressorBuilder);
                }

                if (extension == FileFormats.OGG)
                {
                    return new VorbisWaveReader(filename);
                }

                if (extension == FileFormats.WAV)
                {
                    return new WaveFileReader(filename);
                }

                if (extension == FileFormats.AIFF || extension == FileFormats.AIF)
                {
                    return new AiffFileReader(filename);
                }
            }
            catch (Exception)
            {
                // Fall through: no decoder is available for this file.
            }

            return null;
        }

        private static void StartMediaFoundation()
        {
            if (mediaFoundationStarted)
            {
                return;
            }

            lock (mediaFoundationLock)
            {
                if (!mediaFoundationStarted)
                {
                    MediaFoundationApi.Startup();
                    mediaFoundationStarted = true;
                }
            }
        }
    }
}
