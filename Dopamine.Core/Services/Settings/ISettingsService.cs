namespace Dopamine.Core.Services.Settings
{
    public interface ISettingsService
    {
        bool UseLightTheme { get; set; }
        void Reset();
    }
}
