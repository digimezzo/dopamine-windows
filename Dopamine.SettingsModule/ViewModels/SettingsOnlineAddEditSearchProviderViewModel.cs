using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Provider;
using Dopamine.Core.Utils;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsOnlineAddEditSearchProviderViewModel : BindableBase
    {
        #region Variables
        private SearchProvider provider;
        private IDialogService dialogService;
        private IProviderService providerService;
        #endregion

        #region Properties
        public SearchProvider Provider
        {
            get { return this.provider; }
            set
            {
                SetProperty<SearchProvider>(ref this.provider, value);
            }
        }
        #endregion

        #region Construction
        public SettingsOnlineAddEditSearchProviderViewModel(SearchProvider provider, IDialogService dialogService, IProviderService providerService)
        {
            this.dialogService = dialogService;
            this.providerService = providerService;

            this.provider = provider;
        }
        #endregion

        public async Task<bool> UpdateSearchProviderAsync()
        {
            if (!this.providerService.UpdateSearchProvider(this.provider))
            {
                this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetStringResource("Language_Error"),
                        ResourceUtils.GetStringResource("Language_Error_Adding_Online_Search_Provider"),
                        ResourceUtils.GetStringResource("Language_Ok"),
                        true,
                        ResourceUtils.GetStringResource("Language_Log_File"));

                return false;
            }

            return true;
        }
    }
}
