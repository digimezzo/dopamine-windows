using NAudio.Dsp;
using System;

namespace Dopamine.Core.Audio
{
    /// <summary>
    /// Keeps a rolling window of the most recent samples and exposes the linear FFT magnitudes
    /// (fftLength / 2 values). The FFT is calculated on demand, when the magnitudes are read,
    /// which keeps it off the playback thread and lets the spectrum follow the render rate
    /// instead of the much coarser audio buffer rate.
    /// </summary>
    public class FftAggregator
    {
        private readonly int fftLength;
        private readonly int fftLengthLog2;
        private readonly Complex[] storedSamples;
        private readonly Complex[] transformBuffer;
        private readonly float[] magnitudes;
        private readonly object syncRoot = new object();

        private int position;
        private bool newDataAvailable;

        public FftAggregator(int fftLength)
        {
            if (fftLength < 2 || (fftLength & (fftLength - 1)) != 0)
            {
                throw new ArgumentException("fftLength must be a power of two, at least 2.", nameof(fftLength));
            }

            this.fftLength = fftLength;
            this.fftLengthLog2 = (int)Math.Log(fftLength, 2.0);
            this.storedSamples = new Complex[fftLength];
            this.transformBuffer = new Complex[fftLength];
            this.magnitudes = new float[fftLength / 2];
        }

        public int MagnitudeCount
        {
            get { return this.magnitudes.Length; }
        }

        /// <summary>
        /// Adds a single sample. This is called from the playback thread.
        /// </summary>
        public void Add(float sample)
        {
            lock (this.syncRoot)
            {
                this.storedSamples[this.position].X = sample;
                this.storedSamples[this.position].Y = 0;
                this.position++;

                if (this.position >= this.fftLength)
                {
                    this.position = 0;
                }

                this.newDataAvailable = true;
            }
        }

        /// <summary>
        /// Calculates the FFT of the current window, copies the magnitudes into
        /// <paramref name="destination"/> and reports whether new samples have been added since
        /// the previous call.
        /// </summary>
        public bool GetMagnitudes(float[] destination)
        {
            bool result;

            lock (this.syncRoot)
            {
                // Copy the rolling window in chronological order.
                int head = this.position;

                for (int i = 0; i < this.fftLength; i++)
                {
                    int source = head + i;

                    if (source >= this.fftLength)
                    {
                        source -= this.fftLength;
                    }

                    this.transformBuffer[i] = this.storedSamples[source];
                }

                result = this.newDataAvailable;
                this.newDataAvailable = false;
            }

            // The transform runs outside the lock: it only touches the local buffer.
            for (int i = 0; i < this.fftLength; i++)
            {
                double window = FastFourierTransform.HammingWindow(i, this.fftLength);
                this.transformBuffer[i].X = (float)(this.transformBuffer[i].X * window);
                this.transformBuffer[i].Y = 0;
            }

            FastFourierTransform.FFT(true, this.fftLengthLog2, this.transformBuffer);

            for (int i = 0; i < this.magnitudes.Length; i++)
            {
                float x = this.transformBuffer[i].X;
                float y = this.transformBuffer[i].Y;

                // NAudio's FastFourierTransform already normalizes by the frame size.
                this.magnitudes[i] = (float)Math.Sqrt(x * x + y * y);
            }

            Array.Copy(this.magnitudes, destination, Math.Min(this.magnitudes.Length, destination.Length));
            return result;
        }

        /// <summary>
        /// Index of the given frequency in the magnitudes array.
        /// </summary>
        public int FrequencyToIndex(int frequency, int sampleRate)
        {
            if (sampleRate <= 0)
            {
                return 0;
            }

            double maxFrequency = sampleRate / 2.0;
            int index = Convert.ToInt32(frequency / maxFrequency * this.magnitudes.Length);
            return Math.Min(Math.Max(index, 0), this.magnitudes.Length - 1);
        }
    }
}
