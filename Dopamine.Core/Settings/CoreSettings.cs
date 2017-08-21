using Microsoft.Practices.ServiceLocation;
using System;

namespace Dopamine.Core.Settings
{
    public abstract class CoreSettings : ICoreSettings
    {
        public static ICoreSettings Current
        {
            get
            {
                ICoreSettings coreSettings;

                try
                {
                    coreSettings = ServiceLocator.Current.GetInstance<ICoreSettings>();
                }
                catch (Exception)
                {
                    // Failure to resolve an implementation of ICoreSettings should not break code which require logging.
                    // This is especially useful in unit tests, where logging is not the center of focus.
                    coreSettings = new MockCoreSettings();
                }

                return coreSettings;
            }
        }

        public abstract bool UseLightTheme { get; set; }
        public abstract bool FollowWindowsColor { get; set; }
        public abstract string ColorScheme { get; set; }
        public abstract void Reset();
    }
}
