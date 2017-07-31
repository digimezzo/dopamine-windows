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
        #endregion
    }
}
