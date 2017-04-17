using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Common.Services.Appearance
{
    public interface IAppearanceService
    {
        List<ColorScheme> GetColorSchemes();
        ColorScheme GetColorScheme(string name);
        void ApplyTheme(bool enableLightTheme);
        Task ApplyColorScheme(bool followWindowsColor, bool followAlbumCoverColor, bool isViewModelLoaded = false, string selectedColorScheme = "");
        void WatchWindowsColor(Window window);
        event EventHandler ThemeChanged;
        event EventHandler ColorSchemeChanged;
        event EventHandler ColorSchemesChanged;
    }
}
