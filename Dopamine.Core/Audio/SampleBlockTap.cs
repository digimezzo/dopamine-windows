using NAudio.Wave;
using System;

namespace Dopamine.Core.Audio
{
    /// <summary>
    /// An <see cref="ISampleProvider"/> which passes the samples through unchanged and raises
    /// <see cref="BlockRead"/> for every block read by the output. The spectrum analyzer and the
    /// external control share this single tap.
    /// </summary>
    public class SampleBlockTap : ISampleProvider
    {
        private readonly ISampleProvider source;

        public event EventHandler<AudioBlockReadEventArgs> BlockRead;

        public SampleBlockTap(ISampleProvider source)
        {
            this.source = source;
        }

        public WaveFormat WaveFormat
        {
            get { return this.source.WaveFormat; }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = this.source.Read(buffer, offset, count);

            if (read > 0)
            {
                EventHandler<AudioBlockReadEventArgs> handler = this.BlockRead;

                if (handler != null)
                {
                    handler(this, new AudioBlockReadEventArgs(buffer, offset, read, this.WaveFormat.Channels, this.WaveFormat.SampleRate));
                }
            }

            return read;
        }
    }
}
