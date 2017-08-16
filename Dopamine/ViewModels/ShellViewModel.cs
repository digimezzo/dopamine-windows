using Digimezzo.Utilities.IO;
using Dopamine.Core.Logging;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Presentation.Views;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.File;
using Dopamine.Common.Services.I18n;
using Dopamine.Common.Services.JumpList;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Scrobbling;
using Dopamine.Common.Services.Taskbar;
using Dopamine.ControlsModule.ViewModels;
using Dopamine.ControlsModule.Views;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace Dopamine.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IDialogService dialogService;
        private IPlaybackService playbackService;
        private II18nService i18nService;
        private ITaskbarService taskbarService;
        private IJumpListService jumpListService;
        private IFileService fileService;
        private IScrobblingService scrobblingService;
        private bool isOverlayVisible;
        private IUnityContainer container;
        #endregion

        #region Commands
        public DelegateCommand<string> OpenLinkCommand { get; set; }
        public DelegateCommand<string> OpenPathCommand { get; set; }
        public DelegateCommand PreviousCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }
        public DelegateCommand ShowEqualizerCommand { get; set; }
        public DelegateCommand ShowLogfileCommand { get; set; }
        #endregion

        #region Properties
        public bool IsPreview
        {
            get
            {
#if DEBUG
                return true;
#else
		        return false;
#endif
            }
        }

        public bool IsOverlayVisible
        {
            get { return this.isOverlayVisible; }
            set { SetProperty<bool>(ref this.isOverlayVisible, value); }
        }

        public ITaskbarService TaskbarService
        {
            get { return this.taskbarService; }
            set { this.taskbarService = value; }
        }
        #endregion

        #region Construction
        public ShellViewModel(IUnityContainer container, IRegionManager regionManager, IDialogService dialogService, IPlaybackService playbackService, II18nService i18nService, ITaskbarService taskbarService, IJumpListService jumpListService, IFileService fileService, IScrobblingService scrobblingService)
        {
            this.container = container;
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.playbackService = playbackService;
            this.i18nService = i18nService;
            this.taskbarService = taskbarService;
            this.jumpListService = jumpListService;
            this.fileService = fileService;
            this.scrobblingService = scrobblingService; // Not used here, but needs to be instantiated in the main window to ensure scrobbling is enabled.

            // Event handlers
            this.dialogService.DialogVisibleChanged += isDialogVisible => { this.IsOverlayVisible = isDialogVisible; };

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

            this.OpenPathCommand = new DelegateCommand<string>((string path) =>
            {
                try
                {
                    Actions.TryOpenPath(path);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open the path {0} in Explorer. Exception: {1}", path, ex.Message);
                }
            });
            ApplicationCommands.OpenPathCommand.RegisterCommand(this.OpenPathCommand);

            this.OpenLinkCommand = new DelegateCommand<string>((string link) =>
            {
                try
                {
                    Actions.TryOpenLink(link);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open the link {0} in Internet Explorer. Exception: {1}", link, ex.Message);
                }
            });
            ApplicationCommands.OpenLinkCommand.RegisterCommand(this.OpenLinkCommand);

            this.PreviousCommand = new DelegateCommand(async () => await this.playbackService.PlayPreviousAsync());
            this.NextCommand = new DelegateCommand(async () => await this.playbackService.PlayNextAsync());

            this.LoadedCommand = new DelegateCommand(() => this.fileService.ProcessArguments(Environment.GetCommandLineArgs()));

            this.playbackService.PlaybackFailed += (sender, playbackFailedEventArgs) =>
            {
                switch (playbackFailedEventArgs.FailureReason)
                {
                    case PlaybackFailureReason.FileNotFound:
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Cannot_Play_This_Song_File_Not_Found"), ResourceUtils.GetStringResource("Language_Ok"), false, string.Empty);
                        break;
                    default:
                        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Cannot_Play_This_Song"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
                        break;
                }
            };

            // Equalizer
            this.ShowEqualizerCommand = new DelegateCommand(() =>
            {
                EqualizerControl view = this.container.Resolve<EqualizerControl>();
                view.DataContext = this.container.Resolve<EqualizerControlViewModel>();

                this.dialogService.ShowCustomDialog(
                    new EqualizerIcon() { IsDialogIcon = true },
                    0,
                    ResourceUtils.GetStringResource("Language_Equalizer"),
                    view,
                    570,
                    0,
                    false,
                    true,
                    true,
                    false,
                    ResourceUtils.GetStringResource("Language_Close"),
                    string.Empty,
                    null);
            });

            ApplicationCommands.ShowEqualizerCommand.RegisterCommand(this.ShowEqualizerCommand);

            // Populate the JumpList
            this.jumpListService.PopulateJumpListAsync();
        }
        #endregion
    }

}
