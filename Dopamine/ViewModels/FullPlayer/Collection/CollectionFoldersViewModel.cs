using Digimezzo.Foundation.Core.Settings;
using Dopamine.Services.Entities;
using Dopamine.Services.Folders;
using Dopamine.ViewModels.Common.Base;
using Prism.Ioc;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Collection
{
    public class CollectionFoldersViewModel : TracksViewModelBase
    {
        private IFoldersService foldersService;
        private double leftPaneWidthPercent;
        private ObservableCollection<FolderViewModel> folders;
        private FolderViewModel selectedFolder;

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

        public FolderViewModel SelectedFolder
        {
            get { return this.selectedFolder; }
            set
            {
                SetProperty<FolderViewModel>(ref this.selectedFolder, value);
                SettingsClient.Set<string>("Selections", "SelectedFolder", value != null ? value.Path : string.Empty);
            }
        }

        public CollectionFoldersViewModel(IContainerProvider container, IFoldersService foldersService) : base(container)
        {
            this.foldersService = foldersService;

            // Load settings
            this.LeftPaneWidthPercent = SettingsClient.Get<int>("ColumnWidths", "FoldersLeftPaneWidthPercent");

            // Events
            this.foldersService.FoldersChanged += FoldersService_FoldersChanged;
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

        protected async override Task FillListsAsync()
        {
            await this.GetFoldersAsync();
        }
    }
}
