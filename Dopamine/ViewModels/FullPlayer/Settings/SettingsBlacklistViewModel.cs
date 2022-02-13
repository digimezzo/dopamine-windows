using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Services.Blacklist;
using Dopamine.Services.Dialog;
using Dopamine.Services.Entities;
using Dopamine.Services.Playback;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsBlacklistViewModel : BindableBase
    {
        private IBlacklistService blacklistService;
        private IDialogService dialogService;
        private ObservableCollection<BlacklistTrackViewModel> blacklistTracks;
        private bool isBusy;

        public SettingsBlacklistViewModel(IBlacklistService blacklistService, IDialogService dialogService)
        {
            this.blacklistService = blacklistService;
            this.dialogService = dialogService;

            this.blacklistService.AddedTracksToBacklist += numberOfTracks =>
            {
                this.GetBlacklistTracksAsync();
            };

            this.RemoveBlacklistTrackCommand = new DelegateCommand<long?>(blacklistTrackId =>
            {
                if (this.dialogService.ShowConfirmation(0xe11b, 16, ResourceUtils.GetString("Language_Remove"), ResourceUtils.GetString("Language_Confirm_Remove_Track_From_Blacklist"), ResourceUtils.GetString("Language_Yes"), ResourceUtils.GetString("Language_No")))
                {
                    this.RemoveBlacklistTrack(blacklistTrackId.Value);
                }
            });

            this.ClearBlacklistCommand = new DelegateCommand(() =>
            {
                if (this.dialogService.ShowConfirmation(0xe11b, 16, ResourceUtils.GetString("Language_Clear"), ResourceUtils.GetString("Language_Confirm_Clear_Blacklist"), ResourceUtils.GetString("Language_Yes"), ResourceUtils.GetString("Language_No")))
                {
                    this.RemoveAllFromBlacklistAsync();
                }
            });

            this.GetBlacklistTracksAsync();
        }

        public DelegateCommand<long?> RemoveBlacklistTrackCommand { get; set; }
        public DelegateCommand ClearBlacklistCommand { get; set; }

        public ObservableCollection<BlacklistTrackViewModel> BlacklistTracks
        {
            get { return this.blacklistTracks; }
            set { SetProperty<ObservableCollection<BlacklistTrackViewModel>>(ref this.blacklistTracks, value); }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                SetProperty<bool>(ref this.isBusy, value);
                RaisePropertyChanged(nameof(this.IsBusy));
            }
        }

        private async void RemoveBlacklistTrack(long blacklistTrackId)
        {
            try
            {
                this.IsBusy = true;
                await this.blacklistService.RemoveFromBlacklistAsync(blacklistTrackId);
                this.IsBusy = false;

                this.GetBlacklistTracksAsync();
            }
            catch (Exception ex)
            {
                LogClient.Error("Exception: {0}", ex.Message);

                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Removing_Track_From_Blacklist"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async void RemoveAllFromBlacklistAsync()
        {
            try
            {
                this.IsBusy = true;
                await this.blacklistService.RemoveAllFromBlacklistAsync();
                this.IsBusy = false;

                this.GetBlacklistTracksAsync();
            }
            catch (Exception ex)
            {
                LogClient.Error("Exception: {0}", ex.Message);

                this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetString("Language_Error"), ResourceUtils.GetString("Language_Error_Clearing_Blacklist"), ResourceUtils.GetString("Language_Ok"), true, ResourceUtils.GetString("Language_Log_File"));
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async void GetBlacklistTracksAsync()
        {
            this.IsBusy = true;

            var localBlacklistTracks = new ObservableCollection<BlacklistTrackViewModel>(await this.blacklistService.GetBlacklistTracksAsync());

            this.IsBusy = false;

            this.BlacklistTracks = localBlacklistTracks;
        }
    }
}
