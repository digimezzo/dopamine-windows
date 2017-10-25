using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class ListCheckBox : CheckBox
    {
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

        public static readonly DependencyProperty CheckBackgroundProperty = DependencyProperty.Register("CheckBackground", typeof(Brush), typeof(ListCheckBox), new PropertyMetadata(null));
        public static readonly DependencyProperty CheckBorderBrushProperty = DependencyProperty.Register("CheckBorderBrush", typeof(Brush), typeof(ListCheckBox), new PropertyMetadata(null));
        public static readonly DependencyProperty CheckMarkBrushProperty = DependencyProperty.Register("CheckMarkBrush", typeof(Brush), typeof(ListCheckBox), new PropertyMetadata(null));
       
        static ListCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListCheckBox), new FrameworkPropertyMetadata(typeof(ListCheckBox)));
        }
    }
}
