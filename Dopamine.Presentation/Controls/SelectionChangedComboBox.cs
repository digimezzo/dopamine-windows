using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dopamine.Presentation.Controls
{
    public class SelectionChangedComboBox : ComboBox
    {
        bool mouseWasDown = false;

        public Brush SelectionChangedForeground
        {
            get { return (Brush)GetValue(SelectionChangedForegroundProperty); }

            set { SetValue(SelectionChangedForegroundProperty, value); }
        }

        public static readonly DependencyProperty SelectionChangedForegroundProperty = DependencyProperty.Register("SelectionChangedForeground", typeof(Brush), typeof(SelectionChangedComboBox), new PropertyMetadata(null));

        private void Me_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mouseWasDown)
            {
                this.Foreground = this.SelectionChangedForeground;
            }
        }

        private void Me_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseWasDown = true;
        }
    }
}
