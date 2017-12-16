using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Presentation.Controls
{
    public partial class Logo : UserControl
    {
        public Brush Accent
        {
            get { return (Brush)GetValue(AccentProperty); }

            set { SetValue(AccentProperty, value); }
        }

        public static readonly DependencyProperty AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(Logo), new PropertyMetadata(null));


        public Logo()
        {
            InitializeComponent();
        }
    }
}
