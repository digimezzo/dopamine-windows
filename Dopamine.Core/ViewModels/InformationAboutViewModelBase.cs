using Dopamine.Core.Base;
using Dopamine.Core.Packaging;
using Prism.Mvvm;

namespace Dopamine.Core.ViewModels
{
    public abstract class InformationAboutViewModelBase : BindableBase
    {
        #region Variables
        public abstract ExternalComponent[] Components { get; }
        #endregion

        #region Properties
        public string Copyright => ProductInformation.Copyright;
        public string DonateUrl => ContactInformation.PayPalLink;
        public string WebsiteLink => ContactInformation.WebsiteLink;
        public string WebsiteContactLink => ContactInformation.WebsiteContactLink;
        public string FacebookLink => ContactInformation.FacebookLink;
        public string TwitterLink => ContactInformation.TwitterLink;
        #endregion
    }
}
