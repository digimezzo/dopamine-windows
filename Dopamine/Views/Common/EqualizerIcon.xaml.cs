using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class EqualizerIcon : UserControl
    {
        public bool IsDialogIcon
        {
            get { return (bool)GetValue(IsDialogIconProperty); }
            set { SetValue(IsDialogIconProperty, value); }
        }
   
        public static readonly DependencyProperty IsDialogIconProperty = DependencyProperty.Register("IsDialogIcon", typeof(bool), typeof(EqualizerIcon), new PropertyMetadata(false));
     
        public EqualizerIcon()
        {
            InitializeComponent();
        }
    }
}
