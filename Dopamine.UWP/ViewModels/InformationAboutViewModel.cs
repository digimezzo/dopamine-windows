using Dopamine.UWP.Base;
using Prism.Mvvm;

namespace Dopamine.UWP.ViewModels
{
    public class InformationAboutViewModel : BindableBase
    {
        #region Properties
        public string AssemblyVersion
        {
            get { return ProductInformation.AssemblyVersion; }
        }
        #endregion
    }
}
