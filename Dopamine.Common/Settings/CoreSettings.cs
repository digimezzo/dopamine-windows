using System;

namespace Dopamine.Common.Settings
{
    public class CoreSettings : Core.Settings.CoreSettings
    {
        public override bool UseLightTheme
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<bool>("Appearance", "EnableLightTheme"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<bool>("Appearance", "EnableLightTheme", value); }
        }
        public override bool FollowWindowsColor
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<bool>("Appearance", "FollowWindowsColor"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<bool>("Appearance", "FollowWindowsColor", value); }
        }

        public override string ColorScheme
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<string>("Appearance", "ColorScheme"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<string>("Appearance", "ColorScheme", value); }
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}