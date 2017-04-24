using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using Dopamine.Common.Base;
using Dopamine.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Dopamine.Common.Audio
{
    public class CSCorePlayer : IPlayer, IDisposable
    {
        #region Variables
        // Singleton
        private static CSCorePlayer instance;

        // ISPectrumPlayer
        private List<EventHandler<SingleBlockReadEventArgs>> inputStreamList = new List<EventHandler<SingleBlockReadEventArgs>>();

        // IPlayer
        private string filename;
        private bool canPlay;
        private bool canPause;
        private bool canStop;

        // Output device
        private int latency = 100; // Default is 100
        private bool eventSync = false; // Default is False
        private AudioClientShareMode audioClientShareMode = AudioClientShareMode.Shared; // Default is Shared
        private ISoundOut soundOut;
        private SingleBlockNotificationStream notificationSource;
        private float volume = 1.0F;

        // Equalizer
        private CSCore.Streams.Effects.Equalizer equalizer;
        private double[] filterValues;

        // Flags
        private bool isPlaying;
        #endregion

        #region Construction
        public CSCorePlayer()
        {
            // Register the NVorbis new codec
            CodecFactory.Instance.Register("ogg-vorbis", new CodecFactoryEntry((s) => new NVorbisSource(s).ToWaveSource(), ".ogg"));

            this.canPlay = true;
            this.canPause = false;
            this.canStop = false;
        }

        public static CSCorePlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CSCorePlayer();
                }
                return instance;
            }
        }
        #endregion

        #region ReadOnly Properties
        public string Filename
        {
            get { return this.filename; }
        }

        public bool CanPlay
        {
            get { return this.canPlay; }
        }

        public bool CanPause
        {
            get { return this.canPause; }
        }

        public bool CanStop
        {
            get { return this.canStop; }
        }
        #endregion

        #region Events
        public event EventHandler PlaybackFinished = delegate { };
        public event PlaybackInterruptedEventHandler PlaybackInterrupted = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region Public
        public void ApplyFilterValue(int index, double value)
        {
            if (this.equalizer == null) return;

            EqualizerFilter filter = this.equalizer.SampleFilters[index];
            filter.AverageGainDB = (float)(value);
        }

        public void ApplyFilter(double[] filterValues)
        {
            if (filterValues == null) return;

            for (var i = 0; i < filterValues.Length; i++)
            {
                this.ApplyFilterValue(i, filterValues[i]);
            }
        }

        public void SetOutputDevice(int latency, bool eventMode, bool exclusiveMode, double[] filterValues)
        {
            this.latency = latency;
            this.eventSync = eventMode;
            this.filterValues = filterValues;

            if (exclusiveMode)
            {
                this.audioClientShareMode = AudioClientShareMode.Exclusive;
            }
            else
            {
                this.audioClientShareMode = AudioClientShareMode.Shared;
            }
        }

        public TimeSpan GetCurrentTime()
        {
            // Make sure soundOut is not stopped, otherwise we get a NullReferenceException in CSCore.
            if (this.soundOut != null && this.soundOut.PlaybackState != PlaybackState.Stopped && this.soundOut.WaveSource != null)
            {
                return this.soundOut.WaveSource.GetPosition();
            }

            return new TimeSpan(0);
        }

        public TimeSpan GetTotalTime()
        {
            // Make sure soundOut is not stopped, otherwise we get a NullReferenceException in CSCore.
            if (this.soundOut != null && this.soundOut.PlaybackState != PlaybackState.Stopped && this.soundOut.WaveSource != null)
            {
                return this.soundOut.WaveSource.GetLength();
            }

            return new TimeSpan(0);
        }

        public float GetVolume()
        {
            return this.soundOut.Volume;
        }

        public void Pause()
        {
            if (this.CanPause)
            {
                try
                {
                    this.soundOut.Pause();

                    this.IsPlaying = false;

                    this.canPlay = true;
                    this.canPause = false;
                    this.canStop = true;
                }
                catch (Exception)
                {
                    this.Stop();
                    throw;
                }
            }
        }

        public bool Resume()
        {
            if (this.CanPlay)
            {
                try
                {
                    this.soundOut.Play();

                    this.IsPlaying = true;

                    this.canPlay = false;
                    this.canPause = true;
                    this.canStop = true;
                    return true;
                }
                catch (Exception)
                {
                    this.Stop();
                    throw;
                }
            }

            return false;
        }

        public void Play(string filename, MMDevice outputDevice)
        {
            this.filename = filename;

            this.IsPlaying = true;

            this.canPlay = false;
            this.canPause = true;
            this.canStop = true;

            this.InitializeSoundOut(this.GetCodec(this.filename), outputDevice);
            this.ApplyFilter(this.filterValues);
            this.soundOut.Play();
        }

        private IWaveSource GetCodec(string filename)
        {
            IWaveSource waveSource;

            if (System.IO.Path.GetExtension(this.filename.ToLower()) == FileFormats.MP3)
            {
                // For MP3's, we force usage of MediaFoundationDecoder. CSCore uses DmoMp3Decoder 
                // by default. DmoMp3Decoder however is very slow at playing MP3's from a NAS. 
                // So we're using MediaFoundationDecoder until DmoMp3Decoder has improved.
                waveSource = new MediaFoundationDecoder(this.filename);
            }
            else
            {
                // Other file formats are using the default decoders
                waveSource = CodecFactory.Instance.GetCodec(this.filename);
            }

            // If the SampleRate < 32000, make it 32000. The Equalizer's maximum frequency is 16000Hz.
            // The samplerate has to be bigger than 2 * frequency.
            if (waveSource.WaveFormat.SampleRate < 32000) waveSource = waveSource.ChangeSampleRate(32000);

            return waveSource
                .ToSampleSource()
                .AppendSource(this.Create10BandEqualizer, out this.equalizer)
                .ToWaveSource();
        }

        public void SetVolume(float volume)
        {
            try
            {
                if (volume >= 0)
                {
                    this.volume = volume;
                }
                else
                {
                    this.volume = 0;
                }

                if (this.soundOut != null)
                {
                    this.soundOut.Volume = volume;
                }
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        public void Skip(int gotoSeconds)
        {
            try
            {
                this.soundOut.WaveSource.SetPosition(new TimeSpan(0, 0, gotoSeconds));
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        public void Stop()
        {
            this.CloseSoundOut();

            if (this.CanStop)
            {
                this.IsPlaying = false;

                this.canPlay = true;
                this.canPause = false;
                this.canStop = false;
            }
        }
        #endregion

        #region Private
        private void InitializeSoundOut(IWaveSource soundSource, MMDevice outputDevice)
        {
            // SoundOut implementation which plays the sound
            this.soundOut = new WasapiOut(this.eventSync, this.audioClientShareMode, this.latency, ThreadPriority.Highest){Device = outputDevice};
            ((WasapiOut)this.soundOut).StreamRoutingOptions = StreamRoutingOptions.All;

            // Initialize the soundOut 
            this.notificationSource = new SingleBlockNotificationStream(soundSource.ToSampleSource());
            this.soundOut.Initialize(this.notificationSource.ToWaveSource(16));

            if (inputStreamList.Count != 0)
                foreach (var inputStream in inputStreamList)
                {
                    this.notificationSource.SingleBlockRead += inputStream;
                }

            this.soundOut.Stopped += this.SoundOutStoppedHandler;

            this.soundOut.Volume = this.volume;
        }

        private void NotifyPropertyChanged(string info)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void CloseSoundOut()
        {
            if (this.soundOut != null)
            {
                try
                {
                    if (this.notificationSource != null)
                        foreach (var inputStream in inputStreamList)
                        {
                            this.notificationSource.SingleBlockRead -= inputStream;
                        }

                    // Remove the handler because we don't want to trigger this.soundOut.Stopped()
                    // when manually stopping the player. That event should only be triggered
                    // when CSCore reaches the end of the Track by itself.
                    this.soundOut.Stopped -= this.SoundOutStoppedHandler;
                    this.soundOut.Stop();

                    if (this.soundOut.WaveSource != null) this.soundOut.WaveSource.Dispose();
                    if (this.equalizer != null) this.equalizer.Dispose();

                    this.soundOut.Dispose();
                    this.soundOut = null;
                }
                catch (Exception)
                {
                    //Swallow
                }
            }
        }

        public CSCore.Streams.Effects.Equalizer Create10BandEqualizer(ISampleSource source)
        {
            return this.Create10BandEqualizer(source, 18, 0);
        }

        public CSCore.Streams.Effects.Equalizer Create10BandEqualizer(ISampleSource source, int bandWidth, int defaultGain)
        {
            int sampleRate = source.WaveFormat.SampleRate;
            int channels = source.WaveFormat.Channels;

            var sampleFilters = new[]
            {
                new EqualizerChannelFilter(sampleRate, 70, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 180, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 320, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 600, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 1000, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 3000, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 6000, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 12000, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 14000, bandWidth, defaultGain),
                new EqualizerChannelFilter(sampleRate, 16000, bandWidth, defaultGain)
            };

            var equalizer = new CSCore.Streams.Effects.Equalizer(source);
            foreach (EqualizerChannelFilter equalizerChannelFilter in sampleFilters)
            {
                equalizer.SampleFilters.Add(new EqualizerFilter(channels, equalizerChannelFilter));
            }
            return equalizer;
        }
        #endregion

        #region ISpectrumPlayer
        public bool IsPlaying
        {
            get { return this.isPlaying; }
            set
            {
                this.isPlaying = value;
                NotifyPropertyChanged("IsPlaying");
            }
        }

        public ISpectrumPlayer GetWrapperSpectrumPlayer(SpectrumChannel channel)
        {
            return new WrapperSpectrumPlayer(instance, channel, inputStreamList);
        }

        public class WrapperSpectrumPlayer : ISpectrumPlayer
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };

            public CSCorePlayer player;
            private readonly FftProvider fftProvider;
            private readonly ISoundOut soundOut;

            public bool IsPlaying => this.player.isPlaying;

            public WrapperSpectrumPlayer(CSCorePlayer player, SpectrumChannel channel,
                ICollection<EventHandler<SingleBlockReadEventArgs>> inputStreamList)
            {
                this.player = player;
                this.player.PropertyChanged += (_, __) => PropertyChanged(_, __);
                this.soundOut = player.soundOut;

                fftProvider = new FftProvider(2, FftSize.Fft1024);

                if (channel != SpectrumChannel.Stereo)
                {
                    if (channel == SpectrumChannel.Left)
                    {
                        if (this.player.notificationSource != null) this.player.notificationSource.SingleBlockRead += InputStream_LeftSample;
                        inputStreamList.Add(InputStream_LeftSample);
                    }
                    if (channel == SpectrumChannel.Right)
                    {
                        if (this.player.notificationSource != null) this.player.notificationSource.SingleBlockRead += InputStream_RightSample;
                        inputStreamList.Add(InputStream_RightSample);
                    }
                }
                else
                {
                    if (this.player.notificationSource != null) this.player.notificationSource.SingleBlockRead += InputStream_Sample;
                    inputStreamList.Add(InputStream_Sample);
                }
            }

            private void InputStream_Sample(object sender, SingleBlockReadEventArgs e)
            {
                try
                {
                    this.fftProvider.Add(e.Left, e.Right);
                }
                catch (Exception)
                {
                }
            }

            private void InputStream_LeftSample(object sender, SingleBlockReadEventArgs e)
            {
                try
                {
                    this.fftProvider.Add(e.Left, 0f);
                }
                catch (Exception)
                {
                }
            }

            private void InputStream_RightSample(object sender, SingleBlockReadEventArgs e)
            {
                try
                {
                    this.fftProvider.Add(0f, e.Right);
                }
                catch (Exception)
                {
                }
            }

            public bool GetFFTData(ref float[] fftDataBuffer)
            {
                return this.fftProvider.GetFftData(fftDataBuffer);
            }

            public int GetFFTFrequencyIndex(int frequency)
            {
                try
                {
                    double maxFrequency = 0;

                    if (soundOut != null && this.soundOut.WaveSource != null)
                    {
                        maxFrequency = this.soundOut.WaveSource.WaveFormat.SampleRate / 2.0;
                    }
                    else
                    {
                        maxFrequency = 22050;
                    }
                    // Assume a default 44.1 kHz sample rate.
                    return Convert.ToInt32((frequency / maxFrequency) * ((int)this.fftProvider.FftSize / 2));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        #endregion

        #region Event Handlers
        public void SoundOutStoppedHandler(object sender, PlaybackStoppedEventArgs e)
        {
            try
            {
                if (e.Exception != null)
                {
                    if (PlaybackInterrupted != null)
                    {
                        this.PlaybackInterrupted(this, new PlaybackInterruptedEventArgs { Message = e.Exception.Message });
                    }
                }
                else
                {
                    if (PlaybackFinished != null)
                    {
                        this.PlaybackFinished(this, new EventArgs());
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing. It might be that we get in this handler when the application is closed.
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    this.CloseSoundOut();
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CSCorePlayer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}