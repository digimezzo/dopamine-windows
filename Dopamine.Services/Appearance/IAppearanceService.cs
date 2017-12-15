using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.Services.Appearance
{
    public delegate void ThemeChangedEventHandler(bool useLightTheme);

    public interface IAppearanceService
    { 
        List<ColorScheme> GetColorSchemes();
        ColorScheme GetColorScheme(string name);
        void ApplyTheme(bool useLightTheme);
        event ThemeChangedEventHandler ThemeChanged;
        event EventHandler ColorSchemeChanged;
        event EventHandler ColorSchemesChanged;
        Task ApplyColorSchemeAsync(string selectedColorScheme, bool followWindowsColor, bool followAlbumCoverColor);
        void WatchWindowsColor(object window);
        void OnThemeChanged(bool useLightTheme);
        void OnColorSchemeChanged(EventArgs e);
        void OnColorSchemesChanged(EventArgs e);
    }
}