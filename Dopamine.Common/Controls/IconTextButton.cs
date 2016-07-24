using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class IconTextButton : Button
    {
        #region Properties
        public string Glyph
        {
            get { return Convert.ToString(GetValue(GlyphProperty)); }

            set { SetValue(GlyphProperty, value); }
        }

        public double GlyphSize
        {
            get { return Convert.ToDouble(GetValue(GlyphSizeProperty)); }

            set { SetValue(GlyphSizeProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(IconTextButton), new PropertyMetadata(null));
        public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register("GlyphSize", typeof(double), typeof(IconTextButton), new PropertyMetadata(null));
        #endregion

        #region Construction
        static IconTextButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconTextButton), new FrameworkPropertyMetadata(typeof(IconTextButton)));
        }
        #endregion
    }
}
