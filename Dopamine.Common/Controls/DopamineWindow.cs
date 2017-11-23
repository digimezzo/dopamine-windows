using Digimezzo.WPFControls;
using Dopamine.Common.Base;
using System;
using System.Windows;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class DopamineWindow : BorderlessWindows8Window
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

            this.TitleBarHeight = Convert.ToInt32(Constants.DefaultWindowButtonHeight);
            this.InitializeWindow();
        }
    }
}
