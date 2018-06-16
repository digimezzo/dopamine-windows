using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Controls
{
    public class SplitViewRadioButton : RadioButton
    {
        public String Icon
        {
            get { return (String)GetValue(IconProperty); }

            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(SplitViewRadioButton), new PropertyMetadata(null));

        static SplitViewRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitViewRadioButton), new FrameworkPropertyMetadata(typeof(SplitViewRadioButton)));
        }
    }
}
