using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Controls
{
    public class IconButton : Button
    {
        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }

            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(Geometry), typeof(IconButton), new PropertyMetadata(null));

        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }
    }
}
