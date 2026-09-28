using System;

namespace Dopamine.Core.Audio
{
    /// <summary>
    /// Carries a block of interleaved floating point samples read from the playback pipeline.
    /// </summary>
    public class AudioBlockReadEventArgs : EventArgs
    {
        public float[] Buffer { get; }

        public int Offset { get; }

        /// <summary>
        /// Number of floats read (frames * channels).
        /// </summary>
        public int Count { get; }

        public int Channels { get; }

        public int SampleRate { get; }

        public AudioBlockReadEventArgs(float[] buffer, int offset, int count, int channels, int sampleRate)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
            this.Channels = channels;
            this.SampleRate = sampleRate;
        }
    }
}
