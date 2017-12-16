using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Presentation.Controls
{
    public class AccentTextBox : TextBox
    {
        public bool ShowIcon
        {
            get { return Convert.ToBoolean(GetValue(ShowIconProperty)); }

            set { SetValue(ShowIconProperty, value); }
        }

        public string IconGlyph
        {
            get { return Convert.ToString(GetValue(IconGlyphProperty)); }

            set { SetValue(IconGlyphProperty, value); }
        }

        public string IconToolTip
        {
            get { return Convert.ToString(GetValue(IconToolTipProperty)); }

            set { SetValue(IconToolTipProperty, value); }
        }

        public bool ShowAccent
        {
            get { return Convert.ToBoolean(GetValue(ShowAccentProperty)); }

            set { SetValue(ShowAccentProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register("ShowIcon", typeof(bool), typeof(AccentTextBox), new PropertyMetadata(null));
        public static readonly DependencyProperty IconGlyphProperty = DependencyProperty.Register("IconGlyph", typeof(string), typeof(AccentTextBox), new PropertyMetadata(null));
        public static readonly DependencyProperty IconToolTipProperty = DependencyProperty.Register("IconToolTip", typeof(string), typeof(AccentTextBox), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowAccentProperty = DependencyProperty.Register("ShowAccent", typeof(bool), typeof(AccentTextBox), new PropertyMetadata(null));

        static AccentTextBox()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AccentTextBox), new FrameworkPropertyMetadata(typeof(AccentTextBox)));
        }
    }
}
