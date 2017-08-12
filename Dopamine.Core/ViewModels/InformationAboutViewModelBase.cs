using Dopamine.Core.Base;
using Dopamine.Core.Packaging;
using Prism.Mvvm;
#if WINDOWS_UWP
using Dopamine.UWP.Base;
#else
using Dopamine.Common.Base;
#endif

namespace Dopamine.Core.ViewModels
{
    public abstract class InformationAboutViewModelBase : BindableBase
    {
        #region Variables
        private static readonly ProductInformation productInformation = new ProductInformation();
        #endregion

        #region Properties
        public ExternalComponent[] Components => productInformation.Components;
        public string Copyright => ProductInformationBase.Copyright;
        public string DonateUrl => ContactInformation.PayPalLink;
        public string WebsiteLink => ContactInformation.WebsiteLink;
        public string WebsiteContactLink => ContactInformation.WebsiteContactLink;
        public string FacebookLink => ContactInformation.FacebookLink;
        public string TwitterLink => ContactInformation.TwitterLink;
        #endregion
    }
}
