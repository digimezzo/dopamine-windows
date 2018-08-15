using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Prism;
using Dopamine.Data;
using Dopamine.Services.Entities;
using Dopamine.Services.File;
using Dopamine.Services.Folders;
using Dopamine.ViewModels.Common.Base;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionFoldersViewModel : TracksViewModelBase
    {
        private IFoldersService foldersService;
        private IFileService fileService;
        private IEventAggregator eventAggregator;
        private double leftPaneWidthPercent;
        private ObservableCollection<FolderViewModel> folders;
        private ObservableCollection<SubfolderViewModel> subfolders;
        private FolderViewModel selectedFolder;
        private string activeSubfolderPath;

        public double LeftPaneWidthPercent
        {
            get { return this.leftPaneWidthPercent; }
            set
            {
                SetProperty<double>(ref this.leftPaneWidthPercent, value);
                SettingsClient.Set<int>("ColumnWidths", "FoldersLeftPaneWidthPercent", Convert.ToInt32(value));
            }
        }

        public ObservableCollection<FolderViewModel> Folders
        {
            get { return this.folders; }
            set { SetProperty<ObservableCollection<FolderViewModel>>(ref this.folders, value); }
        }

        public ObservableCollection<SubfolderViewModel> Subfolders
        {
            get { return this.subfolders; }
            set { SetProperty<ObservableCollection<SubfolderViewModel>>(ref this.subfolders, value); }
        }

        public FolderViewModel SelectedFolder
        {
            get { return this.selectedFolder; }
            set
            {
                SetProperty<FolderViewModel>(ref this.selectedFolder, value);
                SettingsClient.Set<string>("Selections", "SelectedFolder", value != null ? value.Path : string.Empty);
                this.GetSubfoldersAsync(null);
            }
        }

        

        public string ActiveSubfolderPath
        {
            get { return this.activeSubfolderPath; }
            set
            {
                SetProperty<string>(ref this.activeSubfolderPath, value);
            }
        }


        public CollectionFoldersViewModel(IContainerProvider container, IFoldersService foldersService, IFileService fileService,
            IEventAggregator eventAggregator) : base(container)
        {
            this.foldersService = foldersService;
            this.fileService = fileService;
            this.eventAggregator = eventAggregator;

            // Load settings
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "FoldersLeftPaneWidthPercent");

            // Events
            this.foldersService.FoldersChanged += FoldersService_FoldersChanged;
            this.eventAggregator.GetEvent<ActiveSubfolderChanged>().Subscribe((activeSubfolder) =>
            {
                this.GetSubfoldersAsync(activeSubfolder as SubfolderViewModel);
            });
        }

        private async void FoldersService_FoldersChanged(object sender, EventArgs e)
        {
            await this.FillListsAsync();
        }

        private async Task GetFoldersAsync()
        {
            this.Folders = new ObservableCollection<FolderViewModel>(await this.foldersService.GetFoldersAsync());
            FolderViewModel proposedSelectedFolder = await this.foldersService.GetSelectedFolderAsync();
            this.selectedFolder = this.Folders.Where(x => x.Equals(proposedSelectedFolder)).FirstOrDefault();
            this.RaisePropertyChanged(nameof(this.SelectedFolder));
        }

        private async Task GetSubfoldersAsync(SubfolderViewModel activeSubfolder)
        {
            this.Subfolders = null; // Required to correctly reset the selectedSubfolder
            this.Subfolders = new ObservableCollection<SubfolderViewModel>(await this.foldersService.GetSubfoldersAsync(this.selectedFolder, activeSubfolder));
            this.ActiveSubfolderPath = this.subfolders.Count > 0 && this.subfolders.Any(x => x.IsGoToParent) ? this.subfolders.Where(x => x.IsGoToParent).First().Path : this.selectedFolder.Path;
            await this.GetTracksAsync();
        }

        private async Task GetTracksAsync()
        {
            IList<TrackViewModel> tracks = await this.fileService.ProcessFilesInDirectoryAsync(this.activeSubfolderPath);
            await this.GetTracksCommonAsync(tracks, TrackOrder.None);
        }

        protected async override Task FillListsAsync()
        {
            await this.GetFoldersAsync();
            await this.GetSubfoldersAsync(null);
        }
    }
}
