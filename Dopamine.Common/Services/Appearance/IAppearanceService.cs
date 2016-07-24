using System;
using System.Collections.Generic;
using System.Windows;

namespace Dopamine.Common.Services.Appearance
{
    public interface IAppearanceService
    {
        string ColorSchemesSubDirectory { get; set; }
        List<ColorScheme> GetColorSchemes();
        ColorScheme GetColorScheme(string name);
        void ApplyTheme(bool enableLightTheme);
        void ApplyColorScheme(bool followWindowsColor, string selectedColorScheme = "");
        void WatchWindowsColor(Window window);
        event EventHandler ThemeChanged;
        event EventHandler ColorSchemesChanged;
    }
}
