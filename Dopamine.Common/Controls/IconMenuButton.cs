using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class IconMenuButton : RadioButton
    {
        #region Properties
        public Brush AccentForeground
        {
            get { return (Brush)GetValue(AccentForegroundProperty); }

            set { SetValue(AccentForegroundProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty AccentForegroundProperty = DependencyProperty.Register("AccentForeground", typeof(Brush), typeof(IconMenuButton), new PropertyMetadata(null));
        #endregion

        #region Construction
        static IconMenuButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconMenuButton), new FrameworkPropertyMetadata(typeof(IconMenuButton)));
        }
        #endregion
    }
}
