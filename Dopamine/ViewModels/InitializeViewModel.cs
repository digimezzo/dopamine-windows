using Dopamine.Core.Alex;  //Digimezzo.Foundation.Core.Settings
using Dopamine.Core.Base;
using Prism.Mvvm;

namespace Dopamine.ViewModels
{
    public class InitializeViewModel : BindableBase
    {
        public string InitializeText
        {
            get
            {
                if (SettingsClient.Get<bool>("General", "ShowOobe"))
                {
                    return $"Preparing {ProductInformation.ApplicationName}";
                }

                return $"Updating {ProductInformation.ApplicationName}";
            }
        }   
    }
}
