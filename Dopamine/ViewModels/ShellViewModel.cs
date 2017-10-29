using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.File;
using Dopamine.Common.Services.JumpList;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Taskbar;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        private IDialogService dialogService;
        private IFileService fileService;
        private IJumpListService jumpListService;
        private bool isOverlayVisible;

        public ITaskbarService TaskbarService { get; }

        public bool IsOverlayVisible
        {
            get { return this.isOverlayVisible; }
            set { SetProperty<bool>(ref this.isOverlayVisible, value); }
        }

        public DelegateCommand PlayPreviousCommand { get; set; }
        public DelegateCommand PlayNextCommand { get; set; }
        public DelegateCommand PlayOrPauseCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }

        public ShellViewModel(IPlaybackService playbackService, ITaskbarService taskbarService, IDialogService dialogService,
            IJumpListService jumpListService, IFileService fileService)
        {
            this.TaskbarService = taskbarService;
            this.dialogService = dialogService;
            this.jumpListService = jumpListService;
            this.fileService = fileService;

            this.dialogService.DialogVisibleChanged += isDialogVisible => { this.IsOverlayVisible = isDialogVisible; };

            this.PlayPreviousCommand = new DelegateCommand(async () => await playbackService.PlayPreviousAsync());
            this.PlayNextCommand = new DelegateCommand(async () => await playbackService.PlayNextAsync());
            this.PlayOrPauseCommand = new DelegateCommand(async () => await playbackService.PlayOrPauseAsync());

            this.LoadedCommand = new DelegateCommand(() => this.fileService.ProcessArguments(Environment.GetCommandLineArgs()));

            // Populate the JumpList
            this.jumpListService.PopulateJumpListAsync();
        }
    }
}