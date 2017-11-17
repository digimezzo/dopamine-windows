using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls.Base;
using Dopamine.Common.Base;

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
