using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Presentation.Controls
{
    public class IconTextButton : Button
    {
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

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(IconTextButton), new PropertyMetadata(null));
        public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register("GlyphSize", typeof(double), typeof(IconTextButton), new PropertyMetadata(null));

        static IconTextButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconTextButton), new FrameworkPropertyMetadata(typeof(IconTextButton)));
        }
    }
}
