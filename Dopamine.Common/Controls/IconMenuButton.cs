using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class IconMenuButton : RadioButton
    {
        public Brush AccentForeground
        {
            get { return (Brush)GetValue(AccentForegroundProperty); }

            set { SetValue(AccentForegroundProperty, value); }
        }

        public static readonly DependencyProperty AccentForegroundProperty = DependencyProperty.Register("AccentForeground", typeof(Brush), typeof(IconMenuButton), new PropertyMetadata(null));

        static IconMenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconMenuButton), new FrameworkPropertyMetadata(typeof(IconMenuButton)));
        }
    }
}
