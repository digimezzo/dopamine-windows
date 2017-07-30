namespace Dopamine.UWP.Services.Appearance
{
    public interface IAppearanceService : Core.Services.Appearance.IAppearanceService
    {
        void ApplyColorScheme(bool followWindowsColor, string selectedColorScheme = "");
    }
}
