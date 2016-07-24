using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class SelectionChangedComboBox : ComboBox
    {
        #region Variables
        bool mouseWasDown = false;
        #endregion

        #region Properties
        public Brush SelectionChangedForeground
        {
            get { return (Brush)GetValue(SelectionChangedForegroundProperty); }

            set { SetValue(SelectionChangedForegroundProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty SelectionChangedForegroundProperty = DependencyProperty.Register("SelectionChangedForeground", typeof(Brush), typeof(SelectionChangedComboBox), new PropertyMetadata(null));
        #endregion

        #region Event Handlers
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
        #endregion
    }
}
