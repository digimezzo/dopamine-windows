using Dopamine.Core.Enums;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dopamine.Core.Audio
{
    /// <summary>
    /// NAudio based playback engine. Owns the output device and the decode/volume pipeline and
    /// implements the transport (play/pause/resume/stop/seek/volume) and the finished/interrupted
    /// notifications, mirroring the behaviour of the previous CSCore based player.
    /// </summary>
    public class NAudioPlayer : IPlayer, INotifyPropertyChanged, IDisposable
    {
        // Opens the audio files (Media Foundation first, managed decoders as a fallback).
        private readonly IAudioDecoderFactory decoderFactory;

        // Transport state
        private string filename;
        private bool canPlay;
        private bool canPause;
        private bool canStop;
        private bool isPlaying;
        private bool hasMediaFoundationSupport;

        // Pipeline
        private WaveStream decoder;
        private EqualizerSampleProvider equalizer;
        private SampleBlockTap sampleBlockTap;
        private VolumeSampleProvider volumeProvider;
        private IWavePlayer output;

        // Output settings
        private float volume = 1.0f;
        private int latency = 200; // Milliseconds
        private bool eventSync;
        private bool exclusiveMode;
        private bool useAllAvailableChannels;
        private double[] filterValues;

        // Output device
        private MMDevice selectedDevice;
        private IList<MMDevice> mmDevices = new List<MMDevice>();

        // The position and duration are frozen while the output is paused.
        private TimeSpan currentTimeBeforePause;
        private TimeSpan totalTimeBeforePause;
        private bool isStoppedBecausePaused;

        private bool disposedValue;

        public NAudioPlayer(bool hasMediaFoundationSupport)
        {
            this.hasMediaFoundationSupport = hasMediaFoundationSupport;
            this.decoderFactory = new NAudioDecoderFactory(hasMediaFoundationSupport);

            this.canPlay = true;
            this.canPause = false;
            this.canStop = false;
        }

        public event EventHandler PlaybackFinished = delegate { };
        public event PlaybackInterruptedEventHandler PlaybackInterrupted = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event EventHandler<AudioBlockReadEventArgs> AudioBlockRead = delegate { };

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

        public bool IsPlaying
        {
            get { return this.isPlaying; }
            private set
            {
                this.isPlaying = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.IsPlaying)));
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

            this.InitializeOutput(this.decoderFactory.Open(filename));
            this.output.Play();
        }

        private void InitializeOutput(WaveStream decoder)
        {
            this.decoder = decoder;

            ISampleProvider sampleProvider = decoder.ToSampleProvider();

            // 10 band equalizer.
            this.equalizer = new EqualizerSampleProvider(sampleProvider);
            this.equalizer.SetGains(this.filterValues);

            // The shared sample block tap: the spectrum analyzer and the external control FFT
            // export both subscribe to it.
            this.sampleBlockTap = new SampleBlockTap(this.equalizer);
            this.sampleBlockTap.BlockRead += this.SampleBlockTapBlockRead;

            this.volumeProvider = new VolumeSampleProvider(this.sampleBlockTap) { Volume = this.volume };

            this.output = this.CreateOutputDevice();
            this.output.Init(this.volumeProvider.ToWaveProvider());
            this.output.PlaybackStopped += this.OutputPlaybackStopped;
        }

        private void SampleBlockTapBlockRead(object sender, AudioBlockReadEventArgs e)
        {
            this.AudioBlockRead(this, e);
        }

        // The audio buffer bounds the granularity of the sample block tap: the finer the buffer,
        // the smoother the spectrum analyzer. WASAPI and waveOut both buffer internally, so a
        // small buffer is safe; the configured latency is capped to keep the tap fine grained.
        private const int MaxOutputLatency = 50;

        private IWavePlayer CreateOutputDevice()
        {
            AudioClientShareMode shareMode = this.exclusiveMode ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared;
            int outputLatency = Math.Min(this.latency, MaxOutputLatency);

            if (this.hasMediaFoundationSupport)
            {
                // WASAPI (event or timer driven, shared or exclusive).
                if (this.selectedDevice == null)
                {
                    // Play on the default device.
                    return new WasapiOut(shareMode, this.eventSync, outputLatency);
                }

                return new WasapiOut(this.selectedDevice, shareMode, this.eventSync, outputLatency);
            }

            // No Media Foundation available: fall back to the waveOut API.
            return new WaveOutEvent { DesiredLatency = outputLatency };
        }

        public void Pause()
        {
            if (!this.CanPause)
            {
                return;
            }

            try
            {
                this.currentTimeBeforePause = this.decoder.CurrentTime;
                this.totalTimeBeforePause = this.decoder.TotalTime;
                this.isStoppedBecausePaused = true;

                this.output.Pause();

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

        public bool Resume()
        {
            if (!this.CanPlay || this.output == null)
            {
                return false;
            }

            try
            {
                this.isStoppedBecausePaused = false;
                this.output.Play();

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

        public void Stop()
        {
            this.CloseOutput();

            if (this.CanStop)
            {
                this.IsPlaying = false;

                this.canPlay = true;
                this.canPause = false;
                this.canStop = false;
            }
        }

        private void CloseOutput()
        {
            if (this.output != null)
            {
                try
                {
                    // Remove the handler first: we don't want a manual stop to be reported as
                    // "playback finished".
                    this.output.PlaybackStopped -= this.OutputPlaybackStopped;
                    this.output.Stop();
                    this.output.Dispose();
                }
                catch (Exception)
                {
                    // Swallow
                }

                this.output = null;
            }

            if (this.decoder != null)
            {
                try
                {
                    this.decoder.Dispose();
                }
                catch (Exception)
                {
                    // Swallow
                }

                this.decoder = null;
            }

            if (this.sampleBlockTap != null)
            {
                this.sampleBlockTap.BlockRead -= this.SampleBlockTapBlockRead;
                this.sampleBlockTap = null;
            }

            this.equalizer = null;
            this.volumeProvider = null;
        }

        public void Skip(int gotoSeconds)
        {
            try
            {
                if (this.decoder == null)
                {
                    return;
                }

                this.decoder.CurrentTime = TimeSpan.FromSeconds(gotoSeconds);

                if (this.isStoppedBecausePaused)
                {
                    this.currentTimeBeforePause = this.decoder.CurrentTime;
                    this.totalTimeBeforePause = this.decoder.TotalTime;
                }
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        public TimeSpan GetCurrentTime()
        {
            if (this.isStoppedBecausePaused)
            {
                return this.currentTimeBeforePause;
            }

            if (this.decoder == null)
            {
                return TimeSpan.Zero;
            }

            TimeSpan currentTime = this.decoder.CurrentTime;
            TimeSpan totalTime = this.decoder.TotalTime;

            // Some decoders (e.g. Media Foundation) keep reporting a position past the duration.
            return currentTime > totalTime ? totalTime : currentTime;
        }

        public TimeSpan GetTotalTime()
        {
            if (this.isStoppedBecausePaused)
            {
                return this.totalTimeBeforePause;
            }

            if (this.decoder != null)
            {
                return this.decoder.TotalTime;
            }

            return TimeSpan.Zero;
        }

        public void SetVolume(float volume)
        {
            try
            {
                this.volume = volume >= 0 ? volume : 0;

                if (this.volumeProvider != null)
                {
                    this.volumeProvider.Volume = this.volume;
                }
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        public float GetVolume()
        {
            return this.volume;
        }

        public void SetPlaybackSettings(int latency, bool eventMode, bool exclusiveMode, double[] filterValues, bool useAllAvailableChannels)
        {
            this.latency = latency;
            this.eventSync = eventMode;
            this.exclusiveMode = exclusiveMode;
            this.filterValues = filterValues;
            this.useAllAvailableChannels = useAllAvailableChannels;
        }

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

        private void SetSelectedAudioDevice(AudioDevice audioDevice)
        {
            // An empty device id means "play on the default device": no need to enumerate.
            if (string.IsNullOrEmpty(audioDevice.DeviceId))
            {
                this.selectedDevice = null;
                return;
            }

            if (this.selectedDevice != null && this.selectedDevice.ID.Equals(audioDevice.DeviceId))
            {
                return;
            }

            if (this.mmDevices == null || this.mmDevices.Count == 0)
            {
                this.GetAllMMDevices();
            }

            this.selectedDevice = null;

            foreach (MMDevice device in this.mmDevices)
            {
                if (device.ID.Equals(audioDevice.DeviceId))
                {
                    this.selectedDevice = device;
                    break;
                }
            }
        }

        private void GetAllMMDevices()
        {
            this.selectedDevice = null;
            this.DisposeMMDevices();

            this.mmDevices = new List<MMDevice>();

            using (var enumerator = new MMDeviceEnumerator())
            {
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    this.mmDevices.Add(device);
                }
            }
        }

        // MMDevice wraps a COM object: release the wrappers instead of leaving them to the
        // finalizer.
        private void DisposeMMDevices()
        {
            if (this.mmDevices == null)
            {
                return;
            }

            foreach (MMDevice device in this.mmDevices)
            {
                try
                {
                    device.Dispose();
                }
                catch (Exception)
                {
                    // Swallow
                }
            }

            this.mmDevices.Clear();
        }

        public IList<AudioDevice> GetAllAudioDevices()
        {
            IList<AudioDevice> audioDevices = new List<AudioDevice>();

            this.GetAllMMDevices();

            foreach (MMDevice device in this.mmDevices)
            {
                audioDevices.Add(new AudioDevice(device.FriendlyName, device.ID));
            }

            return audioDevices;
        }

        private void OutputPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (this.isStoppedBecausePaused)
            {
                return;
            }

            try
            {
                if (e.Exception != null)
                {
                    this.PlaybackInterrupted(this, new PlaybackInterruptedEventArgs { Message = e.Exception.Message });
                }
                else
                {
                    this.PlaybackFinished(this, EventArgs.Empty);
                }
            }
            catch (Exception)
            {
                // Do nothing. It might be that we get in this handler when the application is closed.
            }
        }

        public void ApplyFilterValue(int index, double value)
        {
            if (this.equalizer != null)
            {
                this.equalizer.SetBandGain(index, value);
            }
        }

        public void ApplyFilter(double[] filterValues)
        {
            this.filterValues = filterValues;

            if (this.equalizer != null)
            {
                this.equalizer.SetGains(filterValues);
            }
        }

        public ISpectrumPlayer GetWrapperSpectrumPlayer(SpectrumChannel channel)
        {
            return new SpectrumPlayer(this, channel);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.CloseOutput();
                    this.DisposeMMDevices();
                    this.selectedDevice = null;
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
