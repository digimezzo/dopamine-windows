using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class IconButton : Button
    {
        #region Properties
        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }

            set { SetValue(DataProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(Geometry), typeof(IconButton), new PropertyMetadata(null));
        #endregion

        #region Construction
        static IconButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }
        #endregion
    }
}
