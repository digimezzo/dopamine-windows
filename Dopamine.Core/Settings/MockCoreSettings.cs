namespace Dopamine.Core.Settings
{
    public class MockCoreSettings : ICoreSettings
    {
        public bool UseLightTheme { get => false; set => value = false; }
        public bool FollowWindowsColor { get => false; set => value = false; }
        public string ColorScheme { get => "Blue"; set => value = "Blue"; }

        public void Reset()
        {
        }
    }
}
