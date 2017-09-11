namespace Dopamine.Core.Settings
{
    public abstract class CoreSettings : ICoreSettings
    {
        public abstract bool UseLightTheme { get; set; }
        public abstract bool FollowWindowsColor { get; set; }
        public abstract string ColorScheme { get; set; }
    }
}
