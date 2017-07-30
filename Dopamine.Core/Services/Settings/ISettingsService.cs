namespace Dopamine.Core.Services.Settings
{
    public interface ISettingsService
    {
        bool UseLightTheme { get; set; }
        bool FollowWindowsColor { get; set; }
        string ColorScheme { get; set; }
        void Reset();
    }
}
