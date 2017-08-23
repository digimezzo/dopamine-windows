namespace Dopamine.UWP.Settings
{
    public class SettingsClient
    {
        private Windows.Storage.ApplicationDataContainer settings;
        private static SettingsClient instance;

        private SettingsClient()
        {
            this.Initialize();
        }

        public static SettingsClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsClient();
                }
                return instance;
            }
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

        public static void Reset()
        {
            SettingsClient.Instance.settings.Values.Clear();
            SettingsClient.Instance.Initialize();
        }

        public static bool UseLightTheme
        {
            get
            {
                return (bool)SettingsClient.Instance.settings.Values["UseLightTheme"];
            }
            set
            {
                SettingsClient.Instance.settings.Values["UseLightTheme"] = value;
            }
        }

        public static bool FollowWindowsColor
        {
            get
            {
                return (bool)SettingsClient.Instance.settings.Values["FollowWindowsColor"];
            }
            set
            {
                SettingsClient.Instance.settings.Values["FollowWindowsColor"] = value;
            }
        }

        public static string ColorScheme
        {
            get
            {
                return (string)SettingsClient.Instance.settings.Values["ColorScheme"];
            }
            set
            {
                SettingsClient.Instance.settings.Values["ColorScheme"] = value;
            }
        }
    }
}
