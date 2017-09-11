using Dopamine.Core.Settings;

namespace Dopamine.Common.Settings
{
    public interface IMergedSettings : ICoreSettings
    {
        bool FollowAlbumCoverColor { get; set; }
    }
}
