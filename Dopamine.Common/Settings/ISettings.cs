using System;

namespace Dopamine.Common.Settings
{
    public delegate void RefreshCollectionAutomaticallyChangedHandler(bool refreshCollectionAutomatically);

    public interface ISettings
    {
        bool UseLightTheme { get; set; }
        bool FollowWindowsColor { get; set; }
        string ColorScheme { get; set; }
        string ApplicationFolder { get; }
        bool FollowAlbumCoverColor { get; set; }
        int LyricsTimeoutSeconds { get; set; }
        string LyricsProviders { get; set; }
        string SelectedEqualizerPreset { get; set; }
        string ManualEqualizerPreset { get; set; }
        bool EnableExternalControl { get; set; }
        bool ShowTrackArtOnPlaylists { get; set; }
        bool RefreshCollectionAutomatically { get; set; }
        bool IgnoreRemovedFiles { get; set; }

        event EventHandler ShowTrackArtOnPlaylistsChanged;
        event RefreshCollectionAutomaticallyChangedHandler RefreshCollectionAutomaticallyChanged;
    }
}
