using Dopamine.Core.ViewModels;
using Dopamine.UWP.Base;

namespace Dopamine.UWP.ViewModels
{
    public sealed class InformationAboutViewModel : InformationAboutViewModelBase
    {
        public string AssemblyVersion => ProductInformation.AssemblyVersion;
    }
}
