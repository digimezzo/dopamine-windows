using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
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
