using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public partial class FullLogo : UserControl
    {
        #region Properties
        public Brush Accent
        {
            get { return (Brush)GetValue(AccentProperty); }

            set { SetValue(AccentProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(FullLogo), new PropertyMetadata(null));
        #endregion

        #region Construction
        public FullLogo()
        {
            InitializeComponent();
        }
        #endregion
    }
}
