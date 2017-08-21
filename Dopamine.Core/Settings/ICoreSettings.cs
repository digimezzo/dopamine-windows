namespace Dopamine.Core.Settings
{
    public interface ICoreSettings
    {
        bool UseLightTheme { get; set; }
        bool FollowWindowsColor { get; set; }

        string ColorScheme { get; set; }

        void Reset();
    }
}
