using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class CombiLabel : Label
    {
        #region Dependency Properties
        public static readonly DependencyProperty FontSize2Property = DependencyProperty.Register("FontSize2", typeof(int), typeof(CombiLabel), new PropertyMetadata(null));
        public static readonly DependencyProperty FontWeight2Property = DependencyProperty.Register("FontWeight2", typeof(FontWeight), typeof(CombiLabel), new PropertyMetadata(null));
        public static readonly DependencyProperty FontStyle2Property = DependencyProperty.Register("FontStyle2", typeof(FontStyle), typeof(CombiLabel), new PropertyMetadata(null));
        public static readonly DependencyProperty Content2Property = DependencyProperty.Register("Content2", typeof(object), typeof(CombiLabel), new PropertyMetadata(null));
        public static readonly DependencyProperty Foreground2Property = DependencyProperty.Register("Foreground2", typeof(Brush), typeof(CombiLabel), new PropertyMetadata(null));
        #endregion

        #region Properties
        public int FontSize2
        {
            get { return Convert.ToInt32(GetValue(FontSize2Property)); }

            set { SetValue(FontSize2Property, value); }
        }

        public FontWeight FontWeight2
        {
            get { return (FontWeight)GetValue(FontWeight2Property); }

            set { SetValue(FontWeight2Property, value); }
        }

        public object Content2
        {
            get { return (object)GetValue(Content2Property); }

            set { SetValue(Content2Property, value); }
        }

        public Brush Foreground2
        {
            get { return (Brush)GetValue(Foreground2Property); }

            set { SetValue(Foreground2Property, value); }
        }
        #endregion

        #region Construction
        static CombiLabel()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CombiLabel), new FrameworkPropertyMetadata(typeof(CombiLabel)));
        }
        #endregion
    }
}
