using Dopamine.Core.Settings;

namespace Dopamine.Common.Settings
{
    public interface IMergedSettings : ICoreSettings
    {
        bool FollowAlbumCoverColor { get; set; }
        int LyricsTimeoutSeconds { get; set; }
        string LyricsProviders { get; set; }
    }
}
