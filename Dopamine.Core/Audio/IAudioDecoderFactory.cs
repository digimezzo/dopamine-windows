using NAudio.Wave;

namespace Dopamine.Core.Audio
{
    public interface IAudioDecoderFactory
    {
        /// <summary>
        /// Opens the given audio file for decoding and returns a seekable stream. Throws when no
        /// decoder is available for the file.
        /// </summary>
        WaveStream Open(string filename);
    }
}
