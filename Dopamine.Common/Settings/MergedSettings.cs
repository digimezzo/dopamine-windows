using System;

namespace Dopamine.Common.Settings
{
    public class MergedSettings : IMergedSettings
    {
        public string ApplicationFolder
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.ApplicationFolder(); }
        }

        public bool UseLightTheme
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<bool>("Appearance", "EnableLightTheme"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<bool>("Appearance", "EnableLightTheme", value); }
        }
        public bool FollowWindowsColor
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<bool>("Appearance", "FollowWindowsColor"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<bool>("Appearance", "FollowWindowsColor", value); }
        }

        public string ColorScheme
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<string>("Appearance", "ColorScheme"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<string>("Appearance", "ColorScheme", value); }
        }

        public bool FollowAlbumCoverColor
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<bool>("Appearance", "FollowAlbumCoverColor"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<bool>("Appearance", "FollowAlbumCoverColor", value); }
        }

        public int LyricsTimeoutSeconds
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<int>("Lyrics", "TimeoutSeconds"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<int>("Lyrics", "TimeoutSeconds", value); }
        }
        public string LyricsProviders
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<string>("Lyrics", "Providers"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<string>("Lyrics", "Providers", value); }
        }

        public string SelectedEqualizerPreset
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<string>("Equalizer", "SelectedPreset"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<string>("Equalizer", "SelectedPreset", value); }
        }

        public string ManualEqualizerPreset
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<string>("Equalizer", "ManualPreset"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<string>("Equalizer", "ManualPreset", value); }
        }

        public bool EnableExternalControl
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<bool>("Playback", "EnableExternalControl"); }
            set { Digimezzo.Utilities.Settings.SettingsClient.Set<bool>("Playback", "EnableExternalControl", value); }
        }

        public bool ShowTrackArtOnPlaylists
        {
            get { return Digimezzo.Utilities.Settings.SettingsClient.Get<bool>("Appearance", "ShowTrackArtOnPlaylists"); }
            set {
                Digimezzo.Utilities.Settings.SettingsClient.Set<bool>("Appearance", "ShowTrackArtOnPlaylists", value);
                this.ShowTrackArtOnPlaylistsChanged(this, new EventArgs());
            }
        }

        public MergedSettings()
        {
            this.Initialize();
        }

        public event EventHandler ShowTrackArtOnPlaylistsChanged = delegate { };

        private void Initialize()
        {
            if (Digimezzo.Utilities.Settings.SettingsClient.IsUpgradeNeeded())
            {
                Digimezzo.Utilities.Settings.SettingsClient.Upgrade();
            }
        }
    }
}