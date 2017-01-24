using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Presentation.ViewModels.Entities;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Provider;
using Dopamine.Common.Services.Search;
using Dopamine.Common.Utils;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public abstract class ViewModelBase : BindableBase
    {
        #region Variables
        // UnityContainer
        protected IUnityContainer container;

        // EventAggregator
        protected IEventAggregator eventAggregator;

        // Services
        protected IProviderService providerService;
        protected IPlaybackService playbackService;
        protected IDialogService dialogService;
        protected ISearchService searchService;

        // Flags
        private bool enableRating;
        private bool enableLove;
        protected bool isFirstLoad = true;

        // Counts
        private long tracksCount;
        protected long totalDuration;
        protected long totalSize;

        // Collections
        private ObservableCollection<SearchProvider> contextMenuSearchProviders;
        #endregion

        #region Commands
        public DelegateCommand<string> SearchOnlineCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }
        #endregion

        #region Properties
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

        public long TracksCount
        {
            get { return this.tracksCount; }
            set { SetProperty<long>(ref this.tracksCount, value); }
        }

        public string TotalSizeInformation
        {
            get { return this.totalSize > 0 ? FormatUtils.FormatFileSize(this.totalSize, false) : string.Empty; }
        }

        public string TotalDurationInformation
        {
            get { return this.totalDuration > 0 ? FormatUtils.FormatDuration(this.totalDuration) : string.Empty; }
        }

        public bool HasContextMenuSearchProviders
        {
            get { return this.ContextMenuSearchProviders != null && this.ContextMenuSearchProviders.Count > 0; }
        }

        public ObservableCollection<SearchProvider> ContextMenuSearchProviders
        {
            get { return this.contextMenuSearchProviders; }
            set
            {
                SetProperty<ObservableCollection<SearchProvider>>(ref this.contextMenuSearchProviders, value);
                OnPropertyChanged(() => this.HasContextMenuSearchProviders);
            }
        }
        #endregion

        #region Construction
        public ViewModelBase(IUnityContainer container)
        {
            // UnityContainer
            this.container = container;

            // EventAggregator
            this.eventAggregator = container.Resolve<IEventAggregator>();

            // Services
            this.providerService = container.Resolve<IProviderService>();
            this.playbackService = container.Resolve<IPlaybackService>();
            this.dialogService = container.Resolve<IDialogService>();
            this.searchService = container.Resolve<ISearchService>();
            
            // Flags
            this.EnableRating = SettingsClient.Get<bool>("Behaviour", "EnableRating");
            this.EnableLove = SettingsClient.Get<bool>("Behaviour", "EnableLove");

            // Commands
            this.LoadedCommand = new DelegateCommand(async () => await this.LoadedCommandAsync());

            // Handlers
            this.providerService.SearchProvidersChanged += (_, __) => { this.GetSearchProvidersAsync(); };

            // Initialize the search providers in the ContextMenu
            this.GetSearchProvidersAsync();
        }
        #endregion

        #region Private
        private async void GetSearchProvidersAsync()
        {
            this.ContextMenuSearchProviders = null;

            List<SearchProvider> providersList = await this.providerService.GetSearchProvidersAsync();
            var localProviders = new ObservableCollection<SearchProvider>();

            await Task.Run(() =>
            {
                foreach (SearchProvider vp in providersList)
                {
                    localProviders.Add(vp);
                }
            });

            this.ContextMenuSearchProviders = localProviders;
        }
        #endregion

        #region Protected
        protected async void SetSizeInformationAsync(CollectionViewSource source)
        {
            // Reset duration and size
            this.totalDuration = 0;
            this.totalSize = 0;

            CollectionView viewCopy = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (source != null)
                {
                    // Create copy of CollectionViewSource because only STA can access it
                    viewCopy = new CollectionView(source.View);
                }
            });

            if (viewCopy != null)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        foreach (TrackViewModel vm in viewCopy)
                        {
                            this.totalDuration += vm.Track.Duration.Value;
                            this.totalSize += vm.Track.FileSize.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("An error occured while setting size information. Exception: {0}", ex.Message);
                    }

                });
            }

            OnPropertyChanged(() => this.TotalDurationInformation);
            OnPropertyChanged(() => this.TotalSizeInformation);
        }

        protected void PerformSearchOnline(string id, string artist, string title)
        {
            this.providerService.SearchOnline(id, new string[] { artist, title });
        }

        protected abstract Task LoadedCommandAsync();
        #endregion
    }
}
