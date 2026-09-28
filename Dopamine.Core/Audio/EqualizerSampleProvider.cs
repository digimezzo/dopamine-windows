using NAudio.Dsp;
using NAudio.Wave;
using System;

namespace Dopamine.Core.Audio
{
    /// <summary>
    /// Applies a 10 band graphic equalizer (peaking BiQuad filters) to a sample pipeline. The
    /// band frequencies match the previous CSCore based equalizer.
    /// </summary>
    public class EqualizerSampleProvider : ISampleProvider
    {
        // The centre frequencies of the 10 bands.
        private static readonly float[] BandFrequencies = { 70f, 180f, 320f, 600f, 1000f, 3000f, 6000f, 12000f, 14000f, 16000f };

        // Matches the band width (Q) used by the previous CSCore based equalizer.
        private const float BandQ = 18f;

        private readonly ISampleProvider source;
        private readonly int channels;
        private readonly float sampleRate;
        private readonly BiQuadFilter[][] filters; // [band][channel]
        private readonly bool[] bandActive;

        private int channelIndex;

        public EqualizerSampleProvider(ISampleProvider source)
        {
            this.source = source;
            this.channels = source.WaveFormat.Channels;
            this.sampleRate = source.WaveFormat.SampleRate;

            this.filters = new BiQuadFilter[BandFrequencies.Length][];
            this.bandActive = new bool[BandFrequencies.Length];

            for (int band = 0; band < BandFrequencies.Length; band++)
            {
                this.filters[band] = new BiQuadFilter[this.channels];

                for (int channel = 0; channel < this.channels; channel++)
                {
                    this.filters[band][channel] = BiQuadFilter.PeakingEQ(this.sampleRate, BandFrequencies[band], BandQ, 0f);
                }
            }
        }

        public int BandCount
        {
            get { return BandFrequencies.Length; }
        }

        public WaveFormat WaveFormat
        {
            get { return this.source.WaveFormat; }
        }

        public void SetGains(double[] gains)
        {
            if (gains == null)
            {
                return;
            }

            for (int band = 0; band < BandFrequencies.Length && band < gains.Length; band++)
            {
                this.SetBandGain(band, gains[band]);
            }
        }

        public void SetBandGain(int bandIndex, double gainDb)
        {
            if (bandIndex < 0 || bandIndex >= this.filters.Length)
            {
                return;
            }

            float gain = (float)gainDb;
            this.bandActive[bandIndex] = gain != 0f;

            if (!this.bandActive[bandIndex])
            {
                // A unity gain band is simply skipped while reading.
                return;
            }

            for (int channel = 0; channel < this.channels; channel++)
            {
                this.filters[bandIndex][channel].SetPeakingEq(this.sampleRate, BandFrequencies[bandIndex], BandQ, gain);
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = this.source.Read(buffer, offset, count);
            int channel = this.channelIndex;

            for (int i = 0; i < read; i++)
            {
                float sample = buffer[offset + i];

                for (int band = 0; band < this.filters.Length; band++)
                {
                    if (this.bandActive[band])
                    {
                        sample = this.filters[band][channel].Transform(sample);
                    }
                }

                buffer[offset + i] = sample;

                channel++;

                if (channel >= this.channels)
                {
                    channel = 0;
                }
            }

            this.channelIndex = channel;
            return read;
        }
    }
}
