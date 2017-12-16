using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Presentation.Controls
{
    public class SyncRing : Label
    {
        public double Middle
        {
            get { return Convert.ToDouble(GetValue(MiddleProperty)); }

            set { SetValue(MiddleProperty, value); }
        }

        public static readonly DependencyProperty MiddleProperty = DependencyProperty.Register("Middle", typeof(double), typeof(SyncRing), new PropertyMetadata(null));

        static SyncRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SyncRing), new FrameworkPropertyMetadata(typeof(SyncRing)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.SizeChanged += SizeChangedHandler;
        }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            this.Middle = this.Width / 2;
        }
    }
}
