using System;

namespace Dopamine.Core.Audio
{
    public delegate void PlaybackInterruptedEventHandler(object sender, PlaybackInterruptedEventArgs e);

    public interface IPlayer
    {
        #region ReadOnly Properties
        bool CanPlay { get; }
        bool CanPause { get; }
        bool CanStop { get; }
        string Filename { get; }
        #endregion

        #region Functions
        void Stop();
        void Play(string filename);
        void Skip(int gotoSeconds);
        void SetVolume(float volume);
        void SetOutputDevice(int latency, bool eventMode, bool exclusiveMode, EqualizerPreset preset);
        void Pause();
        bool Resume();
        float GetVolume();
        TimeSpan GetCurrentTime();
        TimeSpan GetTotalTime();
        void Dispose();
        void SwitchPreset(ref EqualizerPreset preset);
        #endregion

        #region Events
        event EventHandler PlaybackFinished;
        event PlaybackInterruptedEventHandler PlaybackInterrupted;
        #endregion
    }
}
