using Dopamine.Core.Packaging;
using Dopamine.UWP.Base;
using GalaSoft.MvvmLight;

namespace Dopamine.UWP.ViewModels
{
    public class InformationAboutViewModel : ViewModelBase
    {
        #region Properties
        public string AssemblyVersion
        {
            get { return ProductInformation.AssemblyVersion; }
        }

        public string Copyright
        {
            get { return ProductInformation.Copyright; }
        }

        public string DonateUrl
        {
            get { return Core.Base.ContactInformation.PayPalLink; }
        }

        public ExternalComponent[] Components
        {
            get { return ProductInformation.Components; }
        }
        #endregion
    }
}
