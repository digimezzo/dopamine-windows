using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.Ffmpeg;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Base;
using Dopamine.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace Dopamine.Core.Audio
{
    public class CSCorePlayer : IPlayer, IDisposable
    {
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
        private bool useAllAvailableChannels = false;
        private int latency = 100; // Default is 100
        private bool eventSync = false; // Default is False
        private AudioClientShareMode audioClientShareMode = AudioClientShareMode.Shared; // Default is Shared
        private SingleBlockNotificationStream notificationSource;
        private float volume = 1.0F;
        private bool useLogarithmicVolumeScale;
        private ISoundOut soundOut;
        Stream audioStream;

        private MMDevice selectedMMDevice;
        IList<MMDevice> mmDevices = new List<MMDevice>();

        // Equalizer
        private CSCore.Streams.Effects.Equalizer equalizer;
        private double[] filterValues;

        // Flags
        private bool isPlaying;
        private bool hasMediaFoundationSupport = false;

        // To detect redundant calls
        private bool disposedValue = false;

        private TimeSpan currentTimeBeforePause;
        private TimeSpan totalTimeBeforePause;
        private bool isStoppedBecausePaused = false;

        public CSCorePlayer()
        {
            this.canPlay = true;
            this.canPause = false;
            this.canStop = false;

            useLogarithmicVolumeScale = SettingsClient.Get<bool>("Behaviour", "UseLogarithmicVolumeScale");

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Behaviour", "UseLogarithmicVolumeScale"))
                {
                    bool useLogarithmicVolumeScale = (bool)e.Entry.Value;

                    this.UpdateVolumeScale(useLogarithmicVolumeScale);
                }
            };
        }

        private void UpdateVolumeScale(bool useLogarithmicVolume)
        {
            float volume = this.GetVolume();
            this.useLogarithmicVolumeScale = useLogarithmicVolume;
            this.SetVolume(volume);
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

        public SingleBlockNotificationStream NotificationSource => this.notificationSource;

        public bool HasMediaFoundationSupport
        {
            get { return this.hasMediaFoundationSupport; }
            set { this.hasMediaFoundationSupport = value; }
        }

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

        public event EventHandler PlaybackFinished = delegate { };
        public event PlaybackInterruptedEventHandler PlaybackInterrupted = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void SwitchAudioDevice(AudioDevice audioDevice)
        {
            this.SetSelectedAudioDevice(audioDevice);
            bool playerWasPaused = !this.canPause;

            if (this.CanStop)
            {
                TimeSpan oldProgress = this.GetCurrentTime();
                this.Stop();
                this.Play(this.filename, audioDevice);
                this.Skip(Convert.ToInt32(oldProgress.TotalSeconds));

                // The player was paused. Pause it again after switching audio device.
                if (playerWasPaused)
                {
                    this.Pause();
                }
            }
        }

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

        public void SetPlaybackSettings(int latency, bool eventMode, bool exclusiveMode, double[] filterValues, bool useAllAvailableChannels)
        {
            this.useAllAvailableChannels = useAllAvailableChannels;
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
            if (this.isStoppedBecausePaused)
            {
                return this.currentTimeBeforePause;
            }

            // Make sure soundOut is not stopped, otherwise we get a NullReferenceException in CSCore.
            if (this.soundOut != null && this.soundOut.PlaybackState != PlaybackState.Stopped && this.soundOut.WaveSource != null)
            {
                return this.soundOut.WaveSource.GetPosition();
            }

            return new TimeSpan(0);
        }

        public TimeSpan GetTotalTime()
        {
            if (this.isStoppedBecausePaused)
            {
                return this.totalTimeBeforePause;
            }

            // Make sure soundOut is not stopped, otherwise we get a NullReferenceException in CSCore.
            if (this.soundOut != null && this.soundOut.PlaybackState != PlaybackState.Stopped && this.soundOut.WaveSource != null)
            {
                return this.soundOut.WaveSource.GetLength();
            }

            return new TimeSpan(0);
        }

        public float GetVolume()
        {
            if (useLogarithmicVolumeScale) return (float)Math.Pow((double)this.soundOut.Volume, 0.5);
            else return this.soundOut.Volume;
        }

        public void Pause()
        {
            if (this.CanPause)
            {
                try
                {
                    this.currentTimeBeforePause = this.soundOut.WaveSource.GetPosition();
                    this.totalTimeBeforePause = this.soundOut.WaveSource.GetLength();
                    this.isStoppedBecausePaused = true;

                    this.soundOut.Stop();

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
                    this.isStoppedBecausePaused = false;
                    this.soundOut.Play();
                    this.soundOut.WaveSource.SetPosition(this.currentTimeBeforePause);

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

        private void SetSelectedAudioDevice(AudioDevice audioDevice)
        {
            if (this.selectedMMDevice == null || !this.selectedMMDevice.DeviceID.Equals(audioDevice.DeviceId))
            {
                if (this.mmDevices == null || this.mmDevices.Count == 0)
                {
                    this.GetAllMMDevices();
                }

                this.selectedMMDevice = this.mmDevices.Where(x => x.DeviceID.Equals(audioDevice.DeviceId)).FirstOrDefault();
            }
        }

        public void Play(string filename, AudioDevice audioDevice)
        {
            this.isStoppedBecausePaused = false;
            this.SetSelectedAudioDevice(audioDevice);

            this.filename = filename;

            this.IsPlaying = true;

            this.canPlay = false;
            this.canPause = true;
            this.canStop = true;

            this.InitializeSoundOut(this.GetCodec(this.filename));
            this.ApplyFilter(this.filterValues);
            this.soundOut.Play();
        }

        private IWaveSource GetCodec(string filename)
        {
            IWaveSource waveSource = null;
            bool useFfmpegDecoder = true;

            // FfmpegDecoder doesn't support WMA lossless. If Windows Media Foundation is available,
            // we can use MediaFoundationDecoder for WMA files, which supports WMA lossless.
            if (this.hasMediaFoundationSupport && Path.GetExtension(filename).ToLower().Equals(FileFormats.WMA))
            {
                try
                {
                    waveSource = new MediaFoundationDecoder(filename);
                    useFfmpegDecoder = false;
                }
                catch (Exception)
                {
                }
            }

            if (useFfmpegDecoder)
            {
                // waveSource = new FfmpegDecoder(this.filename);

                // On some systems, files with special characters (e.g. "æ", "ø") can't be opened by FfmpegDecoder.
                // This exception is thrown: avformat_open_input returned 0xfffffffe: No such file or directory. 
                // StackTrace: at CSCore.Ffmpeg.FfmpegCalls.AvformatOpenInput(AVFormatContext** formatContext, String url)
                // This issue can't be reproduced for now, so we're using a stream as it works in all cases.
                // See: https://github.com/digimezzo/Dopamine/issues/746
                // And: https://github.com/filoe/cscore/issues/344
                this.audioStream = File.OpenRead(filename);
                waveSource = new FfmpegDecoder(this.audioStream);
            }

            // If the SampleRate < 32000, make it 32000. The Equalizer's maximum frequency is 16000Hz.
            // The sample rate has to be bigger than 2 * frequency.
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
                    if (useLogarithmicVolumeScale) this.volume = (float)Math.Pow((double)volume, 2);
                    else this.volume = volume;
                }
                else
                {
                    this.volume = 0;
                }

                if (this.soundOut != null)
                {
                    if (useLogarithmicVolumeScale) this.soundOut.Volume = (float)Math.Pow((double)volume, 2);
                    else this.soundOut.Volume = volume;
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

                if (this.isStoppedBecausePaused)
                {
                    this.currentTimeBeforePause = this.soundOut.WaveSource.GetPosition();
                    this.totalTimeBeforePause = this.soundOut.WaveSource.GetLength();

                }
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

        private void InitializeSoundOut(IWaveSource soundSource)
        {
            // Create SoundOut
            if (this.hasMediaFoundationSupport)
            {
                this.soundOut = new WasapiOut(this.eventSync, this.audioClientShareMode, this.latency, ThreadPriority.Highest);

                // Map stereo or mono file to all channels
                ((WasapiOut)this.soundOut).UseChannelMixingMatrices = this.useAllAvailableChannels;

                if (this.selectedMMDevice == null)
                {
                    // If no output device was provided, we're playing on the default device.
                    // In such case, we want to detect when the default device changes.
                    // This is done by setting stream routing options
                    ((WasapiOut)this.soundOut).StreamRoutingOptions = StreamRoutingOptions.All;
                }
                else
                {
                    // If an output device was provided, assign it to soundOut.Device.
                    // Only allow stream routing when the device was disconnected.
                    ((WasapiOut)this.soundOut).StreamRoutingOptions = StreamRoutingOptions.OnDeviceDisconnect;
                    ((WasapiOut)this.soundOut).Device = this.selectedMMDevice;
                }

                // Initialize SoundOut 
                this.notificationSource = new SingleBlockNotificationStream(soundSource.ToSampleSource());
                this.soundOut.Initialize(this.notificationSource.ToWaveSource());

                if (inputStreamList.Count != 0)
                {
                    foreach (var inputStream in inputStreamList)
                    {
                        this.notificationSource.SingleBlockRead += inputStream;
                    }
                }
            }
            else
            {
                this.soundOut = new DirectSoundOut(this.latency, ThreadPriority.Highest);

                // Initialize SoundOut
                // Spectrum analyzer performance is only acceptable with WasapiOut,
                // so we're not setting a notificationSource for DirectSoundOut
                this.soundOut.Initialize(soundSource);
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
            // soundOut
            if (this.soundOut != null)
            {
                try
                {
                    if (this.notificationSource != null)
                    {
                        foreach (var inputStream in inputStreamList)
                        {
                            this.notificationSource.SingleBlockRead -= inputStream;
                        }
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

            // audioStream
            if (this.audioStream != null)
            {
                try
                {
                    this.audioStream.Dispose();
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

        public void SoundOutStoppedHandler(object sender, PlaybackStoppedEventArgs e)
        {
            if (this.isStoppedBecausePaused)
            {
                return;
            }

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.CloseSoundOut();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        private void GetAllMMDevices()
        {
            this.mmDevices = new List<MMDevice>();

            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (MMDeviceCollection mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    foreach (var device in mmdeviceCollection)
                    {
                        this.mmDevices.Add(device);
                    }
                }
            }
        }

        public IList<AudioDevice> GetAllAudioDevices()
        {
            IList<AudioDevice> audioDevices = new List<AudioDevice>();

            this.GetAllMMDevices();

            foreach (MMDevice mmDevice in this.mmDevices)
            {
                audioDevices.Add(new AudioDevice(mmDevice.FriendlyName, mmDevice.DeviceID));
            }

            return audioDevices;
        }
    }
}