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
        private string indexingProgress;
        private bool isIndexerRemovingSongs;
        private bool isIndexerAddingSongs;
        private bool isIndexerUpdatingSongs;
        private bool isIndexerUpdatingArtwork;

        public bool IsIndexing
        {
            get { return this.isIndexing; }
            set
            {
                SetProperty<bool>(ref this.isIndexing, value);
            }
        }

        public string IndexingProgress
        {
            get { return this.indexingProgress; }
            set { SetProperty<string>(ref this.indexingProgress, value); }
        }

        public bool IsIndexerRemovingSongs
        {
            get { return this.isIndexerRemovingSongs; }
            set { SetProperty<bool>(ref this.isIndexerRemovingSongs, value); }
        }

        public bool IsIndexerAddingSongs
        {
            get { return this.isIndexerAddingSongs; }
            set { SetProperty<bool>(ref this.isIndexerAddingSongs, value); }
        }

        public bool IsIndexerUpdatingSongs
        {
            get { return this.isIndexerUpdatingSongs; }
            set { SetProperty<bool>(ref this.isIndexerUpdatingSongs, value); }
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
                            this.IsIndexerRemovingSongs = true;
                            this.IsIndexerAddingSongs = false;
                            this.IsIndexerUpdatingSongs = false;
                            this.IndexingProgress = string.Empty;
                            break;
                        case IndexingAction.AddTracks:
                            this.IsIndexerRemovingSongs = false;
                            this.IsIndexerAddingSongs = true;
                            this.IsIndexerUpdatingSongs = false;
                            this.IndexingProgress = this.FillProgress(indexingStatusEventArgs.ProgressCurrent.ToString(), indexingStatusEventArgs.ProgressTotal.ToString());
                            break;
                        case IndexingAction.UpdateTracks:
                            this.IsIndexerRemovingSongs = false;
                            this.IsIndexerAddingSongs = false;
                            this.IsIndexerUpdatingSongs = true;
                            this.IndexingProgress = this.FillProgress(indexingStatusEventArgs.ProgressCurrent.ToString(), indexingStatusEventArgs.ProgressTotal.ToString());
                            break;
                        default:
                            break;
                            // Never happens
                    }
                }
                else
                {
                    this.IndexingProgress = string.Empty;
                }
            });
        }

        private string FillProgress(string currentProgres, string totalProgress)
        {
            string progress = string.Empty;

            progress = "(" + ResourceUtils.GetString("Language_Current_Of_Total") + ")";
            progress = progress.Replace("%current%", currentProgres);
            progress = progress.Replace("%total%", totalProgress);

            return progress;
        }

        private void IndexingService_IndexingStopped(object sender, EventArgs e)
        {
            if (this.IsIndexing)
            {
                this.IsIndexing = false;
                this.IndexingProgress = string.Empty;
            }
        }
    }
}
