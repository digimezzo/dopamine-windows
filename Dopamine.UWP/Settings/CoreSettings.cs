namespace Dopamine.UWP.Settings
{
    public class CoreSettings : Core.Settings.CoreSettings
    {
        #region Variables
        private Windows.Storage.ApplicationDataContainer settings;
        #endregion

        #region Construction
        public CoreSettings()
        {
            this.Initialize();
        }
        #endregion

        #region Private
        private void Initialize()
        {
            // Load the settings
            this.settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Make sure each setting has an initial value
            this.InitializeSetting("UseLightTheme", false);
            this.InitializeSetting("FollowWindowsColor", false);
            this.InitializeSetting("ColorScheme", "Blue");
        }

        private void InitializeSetting(string settingName, object settingValue)
        {
            // If a setting doesn't have an initial value yet, set it.
            if (!this.settings.Values.ContainsKey(settingName))
            {
                this.settings.Values.Add(settingName, settingValue);
            }
        }
        #endregion

        #region Static Properties
        public override bool UseLightTheme
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

        public override bool FollowWindowsColor
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

        public override string ColorScheme
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
        #endregion

        #region override Methods
        public override void Reset()
        {
            this.settings.Values.Clear();
            this.Initialize();
        }
        #endregion
    }
}
