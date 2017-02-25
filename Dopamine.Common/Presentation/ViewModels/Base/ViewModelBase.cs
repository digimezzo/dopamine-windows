using Dopamine.Common.Services.Provider;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels.Base
{
    public abstract class ViewModelBase : BindableBase
    {
        #region Variables
        // UnityContainer
        private IUnityContainer container;

        // Services
        private IProviderService providerService;

        // Collections
        private ObservableCollection<SearchProvider> contextMenuSearchProviders;
        #endregion

        #region Commands
        public DelegateCommand<string> SearchOnlineCommand { get; set; }
        #endregion

        #region Properties
        protected IUnityContainer Container
        {
            get { return container; }
        }

        protected IProviderService ProviderService
        {
            get { return providerService; }
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

            // Services
            this.providerService = container.Resolve<IProviderService>();

            // Commands
            this.SearchOnlineCommand = new DelegateCommand<string>((id) => this.SearchOnline(id));

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
        protected bool HasContextMenuSearchProviders
        {
            get { return this.ContextMenuSearchProviders != null && this.ContextMenuSearchProviders.Count > 0; }
        }
        #endregion

        #region Abstract
        protected abstract void SearchOnline(string id);
        #endregion
    }
}
