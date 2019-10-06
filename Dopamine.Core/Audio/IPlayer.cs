using Dopamine.Core.Enums;
using System;
using System.Collections.Generic;

namespace Dopamine.Core.Audio
{
    public delegate void PlaybackInterruptedEventHandler(object sender, PlaybackInterruptedEventArgs e);

    public interface IPlayer
    {
        bool CanPlay { get; }

        bool CanPause { get; }

        bool CanStop { get; }

        string Filename { get; }

        void Stop();

        void Play(string filename, AudioDevice audioDevice);

        void Skip(int gotoSeconds);

        void SetVolume(float volume);

        void SetPlaybackSettings(int latency, bool eventMode, bool exclusiveMode, double[] filterValues, bool useAllAvailableChannels);

        void Pause();

        bool Resume();

        float GetVolume();

        TimeSpan GetCurrentTime();

        TimeSpan GetTotalTime();

        void Dispose();

        void ApplyFilterValue(int index, double value);

        void ApplyFilter(double[] filterValues);

        ISpectrumPlayer GetWrapperSpectrumPlayer(SpectrumChannel channel);

        void SwitchAudioDevice(AudioDevice audioDevice);

        IList<AudioDevice> GetAllAudioDevices();

        event EventHandler PlaybackFinished;
        event PlaybackInterruptedEventHandler PlaybackInterrupted;
    }
}
