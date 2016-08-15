using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class EqualizerIcon : UserControl
    {
        #region Properties
        public bool IsDialogIcon
        {
            get { return (bool)GetValue(IsDialogIconProperty); }
            set { SetValue(IsDialogIconProperty, value); }
        }
        #endregion

        #region Dependecy Properties
        public static readonly DependencyProperty IsDialogIconProperty = DependencyProperty.Register("IsDialogIcon", typeof(bool), typeof(EqualizerIcon), new PropertyMetadata(false));
        #endregion

        #region Construction
        public EqualizerIcon()
        {
            InitializeComponent();
        }
        #endregion
    }
}
