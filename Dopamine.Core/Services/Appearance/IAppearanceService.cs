using System;
using System.Collections.Generic;

namespace Dopamine.Core.Services.Appearance
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
    }
}
