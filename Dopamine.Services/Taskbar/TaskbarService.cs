using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Core.Base;
using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Contracts.Taskbar;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace Dopamine.Services.Taskbar
{
    public class TaskbarService : BindableBase, ITaskbarService
    {
        private IPlaybackService playbackService;
        private string description;
        private TaskbarItemProgressState progressState;
        private double progressValue;
        private string playPauseText;
        private ImageSource playPauseIcon;

        public string Description
        {
            get { return this.description; }
            private set { SetProperty<string>(ref this.description, value); }
        }

        public TaskbarItemProgressState ProgressState
        {
            get { return this.progressState; }
            private set { SetProperty<TaskbarItemProgressState>(ref this.progressState, value); }
        }

        public double ProgressValue
        {
            get { return this.progressValue; }
            private set { SetProperty<double>(ref this.progressValue, value); }
        }

        public string PlayPauseText
        {
            get { return this.playPauseText; }
            private set { SetProperty<string>(ref this.playPauseText, value); }
        }

        public ImageSource PlayPauseIcon
        {
            get { return this.playPauseIcon; }
            private set { SetProperty<ImageSource>(ref this.playPauseIcon, value); }
        }

        public TaskbarService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.ShowTaskBarItemInfoPause(false);  // When starting, we're not playing yet.

            this.playbackService.PlaybackFailed += (_, __) =>
            {
                this.Description = ProductInformation.ApplicationName;
                this.SetTaskbarProgressState(SettingsClient.Get<bool>("Playback", "ShowProgressInTaskbar"), this.playbackService.IsPlaying);
                this.ShowTaskBarItemInfoPause(false);
            };

            this.playbackService.PlaybackPaused += (_, __) =>
            {
                this.SetTaskbarProgressState(SettingsClient.Get<bool>("Playback", "ShowProgressInTaskbar"), this.playbackService.IsPlaying);
                this.ShowTaskBarItemInfoPause(false);
            };

            this.playbackService.PlaybackResumed += (_, __) =>
            {
                this.SetTaskbarProgressState(SettingsClient.Get<bool>("Playback", "ShowProgressInTaskbar"), this.playbackService.IsPlaying);
                this.ShowTaskBarItemInfoPause(true);
            };

            this.playbackService.PlaybackStopped += (_, __) =>
            {
                this.Description = ProductInformation.ApplicationName;
                this.SetTaskbarProgressState(false, false);
                this.ShowTaskBarItemInfoPause(false);
            };

            this.playbackService.PlaybackSuccess += (_, __) =>
            {
                if (!string.IsNullOrWhiteSpace(this.playbackService.CurrentTrack.Value.ArtistName) && !string.IsNullOrWhiteSpace(this.playbackService.CurrentTrack.Value.TrackTitle))
                {
                    this.Description = this.playbackService.CurrentTrack.Value.ArtistName + " - " + this.playbackService.CurrentTrack.Value.TrackTitle;
                }
                else
                {
                    this.Description = this.playbackService.CurrentTrack.Value.FileName;
                }

                this.SetTaskbarProgressState(SettingsClient.Get<bool>("Playback", "ShowProgressInTaskbar"), this.playbackService.IsPlaying);
                this.ShowTaskBarItemInfoPause(true);
            };

            this.playbackService.PlaybackProgressChanged += (_, __) => { this.ProgressValue = this.playbackService.Progress; };
        }

        private void ShowTaskBarItemInfoPause(bool showPause)
        {
            string value = "Play";

            try
            {
                if (showPause)
                {
                    value = "Pause";
                }

                this.PlayPauseText = Application.Current.TryFindResource("Language_" + value).ToString();

                Application.Current.Dispatcher.Invoke(() => { this.PlayPauseIcon = (ImageSource)new ImageSourceConverter().ConvertFromString("pack://application:,,,/Icons/TaskbarItemInfo_" + value + ".ico"); });
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not change the TaskBarItemInfo Play/Pause icon to '{0}'. Exception: {1}", ex.Message, value);
            }
        }

        private void SetTaskbarProgressState(bool showProgressInTaskbar, bool isPlaying)
        {
            if (showProgressInTaskbar)
            {
                if (isPlaying)
                {
                    this.ProgressState = TaskbarItemProgressState.Normal;
                }
                else
                {
                    this.ProgressState = TaskbarItemProgressState.Paused;
                }
            }
            else
            {
                this.ProgressValue = 0;
                this.ProgressState = TaskbarItemProgressState.None;
            }
        }

        public void SetShowProgressInTaskbar(bool showProgressInTaskbar)
        {
            this.SetTaskbarProgressState(showProgressInTaskbar, this.playbackService.IsPlaying);
        }
    }
}
