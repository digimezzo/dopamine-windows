using System;
using Dopamine.Core.Services.Settings;

namespace Dopamine.UWP.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        #region Variables
        private Windows.Storage.ApplicationDataContainer settings;
        #endregion

        #region Construction
        public SettingsService()
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

        #region ISettingsService
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

        public void Reset()
        {
            this.settings.Values.Clear();
            this.Initialize();
        }
        #endregion
    }
}
