using Dopamine.Core.Settings;

namespace Dopamine.Common.Settings
{
    public interface IMergedSettings : ICoreSettings
    {
        string ApplicationFolder { get; }
        bool FollowAlbumCoverColor { get; set; }
        int LyricsTimeoutSeconds { get; set; }
        string LyricsProviders { get; set; }
        string SelectedEqualizerPreset { get; set; }
        string ManualEqualizerPreset { get; set; }
        bool EnableExternalControl { get; set; }
    }
}
