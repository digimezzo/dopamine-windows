using Digimezzo.WPFControls;
using System.Windows;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class DopamineWindow : BorderlessWindows10Window
    {
        public Brush Accent
        {
            get { return (Brush)GetValue(AccentProperty); }

            set { SetValue(AccentProperty, value); }
        }

        public static readonly DependencyProperty AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(DopamineWindow), new PropertyMetadata(null));

        static DopamineWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DopamineWindow), new FrameworkPropertyMetadata(typeof(DopamineWindow)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.InitializeWindow();
        }
    }
}
