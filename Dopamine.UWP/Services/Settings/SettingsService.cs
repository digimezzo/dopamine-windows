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

        public void Reset()
        {
            this.settings.Values.Clear();
            this.Initialize();
        }
        #endregion
    }
}
