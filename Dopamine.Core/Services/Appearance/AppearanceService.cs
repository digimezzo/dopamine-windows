using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.Core.Services.Appearance
{
    public abstract class AppearanceService : IAppearanceService
    {
        #region Variables
        private bool followWindowsColor = false;
        private List<ColorScheme> colorSchemes = new List<ColorScheme>();
        private ColorScheme[] builtInColorSchemes = {
                                                        new ColorScheme {
                                                            Name = "Blue",
                                                            AccentColor = "#1D7DD4"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Green",
                                                            AccentColor = "#7FB718"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Yellow",
                                                            AccentColor = "#F09609"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Purple",
                                                            AccentColor = "#A835B2"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Pink",
                                                            AccentColor = "#CE0058"
                                                        }
                                                    };
        #endregion

        #region Properties
        public bool FollowWindowsColor
        {
            get { return this.followWindowsColor; }
            set { this.followWindowsColor = value; }
        }
        #endregion

        #region Protected
        protected abstract void GetAllColorSchemes();

        protected void GetBuiltInColorSchemes()
        {
            // For now, we are returning a hard-coded list of themes
            foreach (ColorScheme cs in this.builtInColorSchemes)
            {
                this.colorSchemes.Add(cs);
            }
        }

        protected void AddColorScheme(ColorScheme colorScheme)
        {
            // We don't allow duplicate color scheme names.
            // If already present, remove the previous color scheme which has that name.
            // This allows custom color schemes to override built-in color schemes.
            if (this.colorSchemes.Contains(colorScheme))
            {
                this.colorSchemes.Remove(colorScheme);
            }

            this.colorSchemes.Add(colorScheme);
        }

        protected void ClearColorSchemes()
        {
            this.colorSchemes.Clear();
        }
        #endregion

        #region IAppearanceService
        public event ThemeChangedEventHandler ThemeChanged = delegate { };
        public event EventHandler ColorSchemeChanged = delegate { };
        public event EventHandler ColorSchemesChanged = delegate { };

        public virtual void OnThemeChanged(bool useLightTheme)
        {
            this.ThemeChanged(useLightTheme);
        }

        public virtual void OnColorSchemeChanged(EventArgs e)
        {
            this.ColorSchemeChanged(this, e);
        }

        public virtual void OnColorSchemesChanged(EventArgs e)
        {
            this.ColorSchemesChanged(this, e);
        }

        public ColorScheme GetColorScheme(string name)
        {
            foreach (ColorScheme item in this.colorSchemes)
            {
                if (item.Name.Equals(name))
                {
                    return item;
                }
            }

            // If not found by the loop, return the first color scheme.
            return this.colorSchemes[0];
        }

        public List<ColorScheme> GetColorSchemes()
        {
            return this.colorSchemes.ToList();
        }

        public abstract Task ApplyColorSchemeAsync(string selectedColorScheme, bool followWindowsColor, bool followAlbumCoverColor, bool isViewModelLoaded = false);

        public abstract void ApplyTheme(bool useLightTheme);

        public abstract void WatchWindowsColor(object window);
        #endregion
    }
}