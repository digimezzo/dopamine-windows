using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class ListCheckBox : CheckBox
    {
        #region Properties
        public Brush CheckBackground
        {
            get { return (Brush)GetValue(CheckBackgroundProperty); }

            set { SetValue(CheckBackgroundProperty, value); }
        }

        public Brush CheckBorderBrush
        {
            get { return (Brush)GetValue(CheckBorderBrushProperty); }

            set { SetValue(CheckBorderBrushProperty, value); }
        }

        public Brush CheckMarkBrush
        {
            get { return (Brush)GetValue(CheckMarkBrushProperty); }

            set { SetValue(CheckMarkBrushProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty CheckBackgroundProperty = DependencyProperty.Register("CheckBackground", typeof(Brush), typeof(ListCheckBox), new PropertyMetadata(null));
        public static readonly DependencyProperty CheckBorderBrushProperty = DependencyProperty.Register("CheckBorderBrush", typeof(Brush), typeof(ListCheckBox), new PropertyMetadata(null));
        public static readonly DependencyProperty CheckMarkBrushProperty = DependencyProperty.Register("CheckMarkBrush", typeof(Brush), typeof(ListCheckBox), new PropertyMetadata(null));
        #endregion

        #region Construction
        static ListCheckBox()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListCheckBox), new FrameworkPropertyMetadata(typeof(ListCheckBox)));
        }
        #endregion
    }
}
