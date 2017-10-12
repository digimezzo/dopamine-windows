using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Taskbar;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        public ITaskbarService TaskbarService { get; }

        public DelegateCommand PlayPreviousCommand { get; set; }
        public DelegateCommand PlayNextCommand { get; set; }
        public DelegateCommand PlayOrPauseCommand { get; set; }
        public DelegateCommand ShowLogfileCommand { get; set; }

        public ShellViewModel(IPlaybackService playbackService, ITaskbarService taskbarService)
        {
            this.TaskbarService = taskbarService;

            this.PlayPreviousCommand = new DelegateCommand(async () => await playbackService.PlayPreviousAsync());
            this.PlayNextCommand = new DelegateCommand(async () => await playbackService.PlayNextAsync());
            this.PlayOrPauseCommand = new DelegateCommand(async () => await playbackService.PlayOrPauseAsync());

            this.ShowLogfileCommand = new DelegateCommand(() =>
            {
                try
                {
                    Actions.TryViewInExplorer(LogClient.Logfile());
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not view the log file {0} in explorer. Exception: {1}", LogClient.Logfile(), ex.Message);
                }
            });
        }
    }
}