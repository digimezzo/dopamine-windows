namespace Dopamine.Core.Settings
{
    public class MockCoreSettings : ICoreSettings
    {
        public bool UseLightTheme
        {
            get { return false; }
            set { }
        }

        public bool FollowWindowsColor
        {
            get { return false; }
            set { }
        }

        public string ColorScheme
        {
            get { return "Blue"; }
            set { }
        }

        public void Reset()
        {
        }
    }
}
