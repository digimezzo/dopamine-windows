using Dopamine.Services.Contracts.Dialog;
using Dopamine.Services.Contracts.File;
using Dopamine.Services.Contracts.JumpList;
using Dopamine.Services.Contracts.Playback;
using Dopamine.Services.Contracts.Taskbar;
using Dopamine.Services.Contracts.Update;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        private bool isOverlayVisible;

        public bool IsOverlayVisible
        {
            get { return this.isOverlayVisible; }
            set { SetProperty<bool>(ref this.isOverlayVisible, value); }
        }

        public ITaskbarService TaskbarService { get; }

        public DelegateCommand PlayPreviousCommand { get; set; }
        public DelegateCommand PlayNextCommand { get; set; }
        public DelegateCommand PlayOrPauseCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }

        public ShellViewModel(IPlaybackService playbackService, ITaskbarService taskbarService, IDialogService dialogService,
            IJumpListService jumpListService, IFileService fileService, IUpdateService updateService)
        {
            this.TaskbarService = taskbarService;

            dialogService.DialogVisibleChanged += isDialogVisible => { this.IsOverlayVisible = isDialogVisible; };

            this.PlayPreviousCommand = new DelegateCommand(async () => await playbackService.PlayPreviousAsync());
            this.PlayNextCommand = new DelegateCommand(async () => await playbackService.PlayNextAsync());
            this.PlayOrPauseCommand = new DelegateCommand(async () => await playbackService.PlayOrPauseAsync());

            this.LoadedCommand = new DelegateCommand(() => fileService.ProcessArguments(Environment.GetCommandLineArgs()));

            // Populate the JumpList
            jumpListService.PopulateJumpListAsync();
        }
    }
}