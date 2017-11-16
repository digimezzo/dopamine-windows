using Digimezzo.Utilities.Utils;
using Dopamine.Common.Services.Indexing;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class IndexingStatusViewModel : BindableBase
    {
        private IIndexingService indexingService;
        private bool isIndexing;
        private string indexingStatus;

        public bool IsIndexing
        {
            get { return this.isIndexing; }
            set
            {
                SetProperty<bool>(ref this.isIndexing, value);
            }
        }

        public string IndexingStatus
        {
            get { return this.indexingStatus; }
            set { SetProperty<string>(ref this.indexingStatus, value); }
        }

        public IndexingStatusViewModel(IIndexingService indexingService)
        {
            this.indexingService = indexingService;

            this.indexingService.IndexingStatusChanged += async (indexingStatusEventArgs) => await IndexingService_IndexingStatusChangedAsync(indexingStatusEventArgs);
            this.indexingService.IndexingStopped += IndexingService_IndexingStopped;
        }

        private async Task IndexingService_IndexingStatusChangedAsync(IndexingStatusEventArgs indexingStatusEventArgs)
        {
            await Task.Run(() =>
            {
                this.IsIndexing = this.indexingService.IsIndexing;

                if (this.IsIndexing)
                {
                    switch (indexingStatusEventArgs.IndexingAction)
                    {
                        case IndexingAction.RemoveTracks:
                            this.SetIndexingStatusRemovingTracks();
                            break;
                        case IndexingAction.AddTracks:
                            this.SetIndexingStatusAddingTracks(indexingStatusEventArgs.ProgressCurrent, indexingStatusEventArgs.ProgressPercent);
                            break;
                        case IndexingAction.UpdateTracks:
                            this.SetIndexingStatusUpdatingTracks(indexingStatusEventArgs.ProgressPercent);
                            break;
                        default:
                            break;
                            // Never happens
                    }
                }
                else
                {
                    this.IndexingStatus = string.Empty;
                }
            });
        }

        private void SetIndexingStatusRemovingTracks()
        {
            this.IndexingStatus = ResourceUtils.GetString("Language_Removing_Songs");
        }

        private void SetIndexingStatusAddingTracks(long currentProgress, int progressPercent)
        {
            this.IndexingStatus = $"{ResourceUtils.GetString("Language_Adding_Songs")}: {currentProgress} ({progressPercent}%)";
        }

        private void SetIndexingStatusUpdatingTracks(int progressPercent)
        {
            this.IndexingStatus = $"{ResourceUtils.GetString("Language_Updating_Songs")} ({progressPercent}%)";
        }

        private void IndexingService_IndexingStopped(object sender, EventArgs e)
        {
            if (this.IsIndexing)
            {
                this.IsIndexing = false;
                this.IndexingStatus = string.Empty;
            }
        }
    }
}
