namespace Dopamine.UWP.Settings
{
    public class MergedSettings : IMergedSettings
    {
        private Windows.Storage.ApplicationDataContainer settings;
        
        public MergedSettings()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            // Load the settings
            settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Make sure each setting has an initial value
            this.Initialize("UseLightTheme", false);
            this.Initialize("FollowWindowsColor", false);
            this.Initialize("ColorScheme", "Blue");
        }

        private void Initialize(string settingName, object settingValue)
        {
            // If a setting doesn't have an initial value yet, set it.
            if (!settings.Values.ContainsKey(settingName))
            {
                settings.Values.Add(settingName, settingValue);
            }
        }

        public void Reset()
        {
            this.settings.Values.Clear();
            this.Initialize();
        }

        public bool UseLightTheme
        {
            get
            {
                return (bool)this.settings.Values["UseLightTheme"];
            }
            set
            {
                this.settings.Values["UseLightTheme"] = value;
            }
        }

        public bool FollowWindowsColor
        {
            get
            {
                return (bool)this.settings.Values["FollowWindowsColor"];
            }
            set
            {
                this.settings.Values["FollowWindowsColor"] = value;
            }
        }

        public string ColorScheme
        {
            get
            {
                return (string)this.settings.Values["ColorScheme"];
            }
            set
            {
                this.settings.Values["ColorScheme"] = value;
            }
        }
    }
}
