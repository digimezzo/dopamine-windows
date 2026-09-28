using Dopamine.Core.Enums;
using System;
using System.ComponentModel;

namespace Dopamine.Core.Audio
{
    /// <summary>
    /// Exposes the FFT data of a single spectrum channel. Replaces the previous CSCore based
    /// wrapper and subscribes to the shared sample block tap of the player.
    /// </summary>
    public class SpectrumPlayer : ISpectrumPlayer
    {
        private const int FftLength = 1024;

        private readonly NAudioPlayer player;
        private readonly SpectrumChannel channel;
        private readonly FftAggregator fftAggregator = new FftAggregator(FftLength);
        private readonly EventHandler<AudioBlockReadEventArgs> blockReadHandler;
        private readonly PropertyChangedEventHandler playerPropertyChangedHandler;

        private int sampleRate = 44100;
        private bool isRegistered;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public SpectrumPlayer(NAudioPlayer player, SpectrumChannel channel)
        {
            this.player = player;
            this.channel = channel;

            this.blockReadHandler = this.OnBlockRead;
            this.playerPropertyChangedHandler = (_, e) => this.PropertyChanged(this, e);

            this.Register();
        }

        public bool IsPlaying
        {
            get { return this.player.IsPlaying; }
        }

        private void Register()
        {
            if (this.isRegistered)
            {
                return;
            }

            this.isRegistered = true;

            this.player.AudioBlockRead += this.blockReadHandler;
            this.player.PropertyChanged += this.playerPropertyChangedHandler;
        }

        // Stops feeding this player with audio blocks and releases its subscriptions.
        public void Unregister()
        {
            if (!this.isRegistered)
            {
                return;
            }

            this.isRegistered = false;

            this.player.AudioBlockRead -= this.blockReadHandler;
            this.player.PropertyChanged -= this.playerPropertyChangedHandler;
        }

        private void OnBlockRead(object sender, AudioBlockReadEventArgs e)
        {
            try
            {
                this.sampleRate = e.SampleRate;

                for (int i = 0; i + e.Channels - 1 < e.Count; i += e.Channels)
                {
                    int index = e.Offset + i;
                    float sample;

                    if (e.Channels >= 2)
                    {
                        float left = e.Buffer[index];
                        float right = e.Buffer[index + 1];

                        if (this.channel == SpectrumChannel.Left)
                        {
                            sample = left;
                        }
                        else if (this.channel == SpectrumChannel.Right)
                        {
                            sample = right;
                        }
                        else
                        {
                            sample = 0.5f * (left + right);
                        }
                    }
                    else
                    {
                        sample = e.Buffer[index];
                    }

                    this.fftAggregator.Add(sample);
                }
            }
            catch (Exception)
            {
                // Intended suppression.
            }
        }

        public bool GetFFTData(ref float[] fftDataBuffer)
        {
            return this.fftAggregator.GetMagnitudes(fftDataBuffer);
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            return this.fftAggregator.FrequencyToIndex(frequency, this.sampleRate);
        }
    }
}
