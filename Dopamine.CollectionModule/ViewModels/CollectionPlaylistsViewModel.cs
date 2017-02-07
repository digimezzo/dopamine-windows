using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.CollectionModule.Views;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Dopamine.Common.Database.Repositories.Interfaces;
using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Presentation.ViewModels.Base;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Playlist;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFFolderBrowser;

namespace Dopamine.CollectionModule.ViewModels
{
    public class CollectionPlaylistsViewModel : BindableBase, IDropTarget
    {
        #region Variables
        // EventAggregator
        private IEventAggregator eventAggregator;

        // Services
        private IDialogService dialogService;
        private IPlaylistService playlistService;

        // Lists
        private ObservableCollection<PlaylistViewModel> playlists;
        private IList<string> selectedPlaylists;

        // Flags
        private bool isLoadingPlaylists;
        private bool enableRating;
        private bool enableLove;

        // Other
        private long playlistsCount;
        private double leftPaneWidthPercent;
        #endregion

        #region Commands
        public DelegateCommand NewPlaylistCommand { get; set; }
        public DelegateCommand OpenPlaylistCommand { get; set; }
        public DelegateCommand<string> DeletePlaylistByNameCommand { get; set; }
        public DelegateCommand RenameSelectedPlaylistCommand { get; set; }
        public DelegateCommand DeleteSelectedPlaylistsCommand { get; set; }
        public DelegateCommand<object> SelectedPlaylistsCommand { get; set; }
        public DelegateCommand AddPlaylistsToNowPlayingCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }
        #endregion

        #region Properties
        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public bool EnableRating
        {
            get { return this.enableRating; }
            set { SetProperty<bool>(ref this.enableRating, value); }
        }

        public bool EnableLove
        {
            get { return this.enableLove; }
            set { SetProperty<bool>(ref this.enableLove, value); }
        }

        public bool AllowRename
        {
            get
            {
                if (this.SelectedPlaylists != null)
                {
                    return this.SelectedPlaylists.Count == 1;
                }
                else
                {
                    return false;
                }

                return false;
            }
        }

        public bool IsLoadingPlaylists
        {
            get { return this.isLoadingPlaylists; }
            set { SetProperty<bool>(ref this.isLoadingPlaylists, value); }
        }

        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return this.playlists; }
            set { SetProperty<ObservableCollection<PlaylistViewModel>>(ref this.playlists, value); }
        }

        public IList<string> SelectedPlaylists
        {
            get { return this.selectedPlaylists; }
            set { SetProperty<IList<string>>(ref this.selectedPlaylists, value); }
        }

        public long PlaylistsCount
        {
            get { return this.playlistsCount; }
            set { SetProperty<long>(ref this.playlistsCount, value); }
        }
        #endregion

        #region Construction
        public CollectionPlaylistsViewModel(IEventAggregator eventAggregator, IDialogService dialogService, IPlaylistService playlistService)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.playlistService = playlistService;

            // Commands
            this.LoadedCommand = new DelegateCommand(async () => await this.GetPlaylistsAsync());
            this.NewPlaylistCommand = new DelegateCommand(async () => await this.ConfirmAddPlaylistAsync());
            //this.OpenPlaylistCommand = new DelegateCommand(async () => await this.OpenPlaylistAsync());
            this.DeletePlaylistByNameCommand = new DelegateCommand<string>(async (iPlaylistName) => await this.DeletePlaylistByNameAsync(iPlaylistName));
            this.DeleteSelectedPlaylistsCommand = new DelegateCommand(async () => await this.DeleteSelectedPlaylistsAsync());
            //this.RenameSelectedPlaylistCommand = new DelegateCommand(async () => await this.RenameSelectedPlaylistAsync());
            //this.RemoveSelectedTracksCommand = new DelegateCommand(async () => await this.DeleteTracksFromPlaylistsAsync());
            this.SelectedPlaylistsCommand = new DelegateCommand<object>(async (parameter) => await SelectedPlaylistsHandlerAsync(parameter));
            //this.AddPlaylistsToNowPlayingCommand = new DelegateCommand(async () => await this.AddPLaylistsToNowPlayingAsync(this.SelectedPlaylists));

            // Events
            //this.eventAggregator.GetEvent<RemoveSelectedTracksWithKeyDelete>().Subscribe((screenName) =>
            //{
            //    //if (screenName == typeof(CollectionPlaylists).FullName) this.RemoveSelectedTracksCommand.Execute();
            //});

            this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Subscribe(enableRating => this.EnableRating = enableRating);
            this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Subscribe(enableLove => this.EnableLove = enableLove);

            // CollectionService
            //this.playlistService.AddedTracksToPlaylist += async (_, __) => await this.ReloadPlaylistsAsync();
            //this.playlistService.DeletedTracksFromPlaylists += async (_, __) => await this.ReloadPlaylistsAsync();
            this.playlistService.PlaylistAdded += (addedPlaylist) => this.UpdatePlaylists(addedPlaylist);
            this.playlistService.PlaylistsDeleted += (deletedPlaylists) => this.UpdatePlaylists(deletedPlaylists);

            // Events
            //this.eventAggregator.GetEvent<RenameSelectedPlaylistWithKeyF2>().Subscribe(async (_) => await this.RenameSelectedPlaylistAsync());
            this.eventAggregator.GetEvent<DeleteSelectedPlaylistsWithKeyDelete>().Subscribe(async (_) => await this.DeleteSelectedPlaylistsAsync());

            // Set width of the panels
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "PlaylistsLeftPaneWidthPercent");
        }
        #endregion

        private void UpdatePlaylists(string addedPlaylist)
        {
            this.Playlists.Add(new PlaylistViewModel() { Playlist = addedPlaylist });
        }

        private void UpdatePlaylists(List<string> deletedPlaylists)
        {
            foreach (string deletedPlaylist in deletedPlaylists)
            {
                this.Playlists.Remove(this.Playlists.Where(p => p.Playlist.Equals(deletedPlaylist)).Single());
            }
        }

        #region Private
        private async Task ConfirmAddPlaylistAsync()
        {
            string responseText = ResourceUtils.GetStringResource("Language_New_Playlist");

            if (this.dialogService.ShowInputDialog(
                0xea37,
                16,
                ResourceUtils.GetStringResource("Language_New_Playlist"),
                ResourceUtils.GetStringResource("Language_Enter_Name_For_New_Playlist"),
                ResourceUtils.GetStringResource("Language_Ok"),
                ResourceUtils.GetStringResource("Language_Cancel"),
                ref responseText))
            {
                await this.AddPlaylistAsync(responseText);
            }
        }

        private async Task AddPlaylistAsync(string playlistName)
        {
            AddPlaylistResult result = await this.playlistService.AddPlaylistAsync(playlistName);

            switch (result)
            {
                case AddPlaylistResult.Success:
                    await this.GetPlaylistsAsync();
                    break;
                case AddPlaylistResult.Duplicate:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Already_Exists"),
                        ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name").Replace("%playlistname%", "\"" + playlistName + "\""),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                case AddPlaylistResult.Error:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Adding_Playlist"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));
                    break;
                case AddPlaylistResult.Blank:
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Provide_Playlist_Name"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        false,
                        string.Empty);
                    break;
                default:
                    // Never happens
                    break;
            }
        }

        private async Task GetPlaylistsAsync()
        {
            // Notify the user
            this.IsLoadingPlaylists = true;

            try
            {
                // Get the Albums from the database
                IList<string> playlists = await this.playlistService.GetPlaylistsAsync();

                // Set the count
                this.PlaylistsCount = playlists.Count;

                // Populate an ObservableCollection
                var playlistViewModels = new ObservableCollection<PlaylistViewModel>();

                await Task.Run(() =>
                {
                    foreach (string playlist in playlists)
                    {
                        playlistViewModels.Add(new PlaylistViewModel { Playlist = playlist });
                    }
                });

                // Unbind and rebind to improve UI performance
                this.Playlists = null;
                this.Playlists = playlistViewModels;
            }
            catch (Exception ex)
            {
                LogClient.Error("An error occured while getting Playlists. Exception: {0}", ex.Message);

                // If loading from the database failed, create and empty Collection.
                this.Playlists = new ObservableCollection<PlaylistViewModel>();
            }

            // Stop notifying
            this.IsLoadingPlaylists = false;
        }

        private async Task DeletePlaylistsAsync(IList<string> playlists)
        {
            DeletePlaylistsResult result = await this.playlistService.DeletePlaylistsAsync(playlists);

            await this.GetPlaylistsAsync();

            if (!result.IsSuccess)
            {
                string message = ResourceUtils.GetStringResource("Language_Error_Deleting_Playlists");

                if (playlists.Count == 1)
                {
                    message = ResourceUtils.GetStringResource("Language_Error_Deleting_Playlist").Replace("%playlistname%", "\"" + playlists[0] + "\"");
                }

                this.dialogService.ShowNotification(
                    0xe711,
                    16,
                    ResourceUtils.GetStringResource("Language_Error"),
                    message,
                    ResourceUtils.GetStringResource("Language_Ok"),
                    true,
                    ResourceUtils.GetStringResource("Language_Log_File"));
            }
        }

        private async Task DeleteSelectedPlaylistsAsync()
        {
            if (this.SelectedPlaylists != null && this.SelectedPlaylists.Count > 0)
            {
                string title = ResourceUtils.GetStringResource("Language_Delete");
                string message = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlists");

                if (this.SelectedPlaylists.Count == 1)
                {
                    message = ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist").Replace("%playlistname%", "\"" + this.SelectedPlaylists[0] + "\"");
                }

                if (this.dialogService.ShowConfirmation(
                    0xe11b,
                    16,
                    title,
                    message,
                    ResourceUtils.GetStringResource("Language_Yes"),
                    ResourceUtils.GetStringResource("Language_No")))
                {
                    await this.DeletePlaylistsAsync(this.SelectedPlaylists);
                }
            }
        }

        private async Task DeletePlaylistByNameAsync(string playlist)
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetStringResource("Language_Delete"),
                ResourceUtils.GetStringResource("Language_Are_You_Sure_To_Delete_Playlist").Replace("%playlistname%", "\"" + playlist + "\""),
                ResourceUtils.GetStringResource("Language_Yes"),
                ResourceUtils.GetStringResource("Language_No")))
            {
                var playlists = new List<string>();
                playlists.Add(playlist);

                await this.DeletePlaylistsAsync(playlists);
            }
        }

        private async Task SelectedPlaylistsHandlerAsync(object parameter)
        {
            if (parameter != null)
            {
                this.SelectedPlaylists = new List<string>();

                foreach (PlaylistViewModel item in (IList)parameter)
                {
                    this.SelectedPlaylists.Add(item.Playlist);
                }
                OnPropertyChanged(() => this.AllowRename);
            }

            //await this.GetTracksAsync(this.SelectedPlaylists, this.TrackOrder); // TODO
        }

        //protected async Task AddPLaylistsToNowPlayingAsync(IList<Playlist> playlists)
        //{
        //    EnqueueResult result = await this.playbackService.AddToQueue(playlists);

        //    if (!result.IsSuccess)
        //    {
        //        this.dialogService.ShowNotification(0xe711, 16, ResourceUtils.GetStringResource("Language_Error"), ResourceUtils.GetStringResource("Language_Error_Adding_Playlists_To_Now_Playing"), ResourceUtils.GetStringResource("Language_Ok"), true, ResourceUtils.GetStringResource("Language_Log_File"));
        //    }
        //}



        //public async Task GetTracksAsync(IList<Playlist> selectedPlaylists, TrackOrder trackOrder)
        //{
        //    await this.GetTracksCommonAsync(await this.trackRepository.GetTracksAsync(selectedPlaylists), trackOrder);
        //}



        //private async Task OpenPlaylistAsync()
        //{
        //    // Set up the file dialog box
        //    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        //    dlg.Title = Application.Current.FindResource("Language_Open_Playlist").ToString();
        //    dlg.DefaultExt = FileFormats.M3U; // Default file extension

        //    // Filter files by extension
        //    dlg.Filter = ResourceUtils.GetStringResource("Language_Playlists") + " (*" + FileFormats.M3U + ";*" + FileFormats.WPL + ";*" + FileFormats.ZPL + ")|*" + FileFormats.M3U + ";*" + FileFormats.WPL + ";*" + FileFormats.ZPL;

        //    // Show the file dialog box
        //    bool? dialogResult = dlg.ShowDialog();

        //    // Process the file dialog box result

        //    if ((bool)dialogResult)
        //    {
        //        this.IsLoadingPlaylists = true;

        //        OpenPlaylistResult openResult = await this.playlistService.OpenPlaylistAsync(dlg.FileName);

        //        if (openResult == OpenPlaylistResult.Error)
        //        {
        //            this.IsLoadingPlaylists = false;

        //            this.dialogService.ShowNotification(
        //                0xe711,
        //                16,
        //                ResourceUtils.GetStringResource("Language_Error"),
        //                ResourceUtils.GetStringResource("Language_Error_Opening_Playlist"),
        //                ResourceUtils.GetStringResource("Language_Ok"),
        //                true,
        //                ResourceUtils.GetStringResource("Language_Log_File"));
        //        }
        //    }
        //}









        //private async Task RenameSelectedPlaylistAsync()
        //{
        //    if (this.SelectedPlaylists != null && this.SelectedPlaylists.Count > 0)
        //    {
        //        string oldPlaylistName = this.SelectedPlaylists[0].PlaylistName;
        //        string responseText = oldPlaylistName;

        //        if (this.dialogService.ShowInputDialog(
        //            0xea37,
        //            16,
        //            ResourceUtils.GetStringResource("Language_Rename_Playlist"),
        //            ResourceUtils.GetStringResource("Language_Enter_New_Name_For_Playlist").Replace("%playlistname%", oldPlaylistName),
        //            ResourceUtils.GetStringResource("Language_Ok"),
        //            ResourceUtils.GetStringResource("Language_Cancel"),
        //            ref responseText))
        //        {
        //            await this.RenamePlaylistAsync(oldPlaylistName, responseText);
        //        }
        //    }
        //}

        //private async Task RenamePlaylistAsync(string oldPlaylistName, string newPlaylistName)
        //{
        //    this.IsLoadingPlaylists = true;
        //    RenamePlaylistResult result = await this.playlistService.RenamePlaylistAsync(oldPlaylistName, newPlaylistName);

        //    switch (result)
        //    {
        //        case RenamePlaylistResult.Success:
        //            await this.FillListsAsync();
        //            break;
        //        case RenamePlaylistResult.Duplicate:
        //            this.IsLoadingPlaylists = false;
        //            this.dialogService.ShowNotification(
        //                0xe711,
        //                16,
        //                ResourceUtils.GetStringResource("Language_Already_Exists"),
        //                ResourceUtils.GetStringResource("Language_Already_Playlist_With_That_Name").Replace("%playlistname%", "\"" + newPlaylistName + "\""),
        //                ResourceUtils.GetStringResource("Language_Ok"),
        //                false,
        //                string.Empty);
        //            break;
        //        case RenamePlaylistResult.Error:
        //            this.IsLoadingPlaylists = false;
        //            this.dialogService.ShowNotification(
        //                0xe711,
        //                16,
        //                ResourceUtils.GetStringResource("Language_Error"),
        //                ResourceUtils.GetStringResource("Language_Error_Renaming_Playlist"),
        //                ResourceUtils.GetStringResource("Language_Ok"),
        //                true,
        //                ResourceUtils.GetStringResource("Language_Log_File"));
        //            break;
        //        case RenamePlaylistResult.Blank:
        //            this.IsLoadingPlaylists = false;
        //            this.dialogService.ShowNotification(
        //                0xe711,
        //                16,
        //                ResourceUtils.GetStringResource("Language_Error"),
        //                ResourceUtils.GetStringResource("Language_Provide_Playlist_Name"),
        //                ResourceUtils.GetStringResource("Language_Ok"),
        //                false,
        //                string.Empty);
        //            break;
        //        default:
        //            // Never happens
        //            this.IsLoadingPlaylists = false;
        //            break;
        //    }
        //}

        //private async Task DeleteTracksFromPlaylistsAsync()
        //{
        //    DeleteTracksFromPlaylistsResult result = await this.playlistService.DeleteTracksFromPlaylistAsync(this.SelectedTracks, this.SelectedPlaylists.FirstOrDefault());

        //    switch (result)
        //    {
        //        case DeleteTracksFromPlaylistsResult.Success:
        //            await this.GetTracksAsync(this.SelectedPlaylists, this.TrackOrder);
        //            break;
        //        case DeleteTracksFromPlaylistsResult.Error:
        //            this.dialogService.ShowNotification(
        //                0xe711,
        //                16,
        //                ResourceUtils.GetStringResource("Language_Error"),
        //                ResourceUtils.GetStringResource("Language_Error_Removing_From_Playlist"),
        //                ResourceUtils.GetStringResource("Language_Ok"),
        //                true,
        //                ResourceUtils.GetStringResource("Language_Log_File"));
        //            break;
        //        default:
        //            break;
        //            // Never happens
        //    }
        //}

        //private async Task ReloadPlaylistsAsync()
        //{
        //    if (this.SelectedPlaylists != null && this.SelectedPlaylists.Count > 0)
        //    {
        //        await this.GetTracksAsync(this.SelectedPlaylists, this.TrackOrder);
        //    }
        //}
        #endregion

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            //// We don't allow drag and drop when more as 1 playlist is selected
            //if (this.selectedPlaylists != null && this.selectedPlaylists.Count == 1)
            //{
            //    GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);

            //    try
            //    {
            //        dropInfo.NotHandled = true;
            //    }
            //    catch (Exception ex)
            //    {
            //        dropInfo.NotHandled = false;
            //        LogClient.Error("Could not drag tracks. Exception: {0}", ex.Message);
            //    }
            //}
        }

        public async void Drop(IDropInfo dropInfo)
        {
            //GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            //try
            //{
            //    var tracks = new List<PlayableTrack>();

            //    foreach (var item in dropInfo.TargetCollection)
            //    {
            //        tracks.Add(((TrackViewModel)item).Track);
            //    }

            //    await this.playlistRepository.DeleteTracksFromPlaylistAsync(tracks, selectedPlaylists[0]);
            //    await this.playlistRepository.AddTracksToPlaylistAsync(tracks, selectedPlaylists[0].PlaylistName);
            //}
            //catch (Exception ex)
            //{
            //    LogClient.Error("Could not drop tracks. Exception: {0}", ex.Message);
            //}
        }
        #endregion
    }
}
