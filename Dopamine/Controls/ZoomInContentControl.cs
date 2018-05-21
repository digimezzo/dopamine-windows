using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Dopamine.Controls
{
    public class ZoomInContentControl : ContentControl
    {
        public double Duration
        {
            get { return Convert.ToDouble(GetValue(DurationProperty)); }

            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(double), typeof(ZoomInContentControl), new PropertyMetadata(0.5));

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (newContent != null)
            {
                this.DoAnimation();
            }
        }

        private void DoAnimation()
        {
            var ta = new ThicknessAnimation();
            ta.From = new Thickness(this.ActualWidth / 2, this.ActualHeight / 2, this.ActualWidth / 2, this.ActualHeight / 2);
            ta.To = new Thickness(0, 0, 0, 0);
            ta.Duration = new Duration(TimeSpan.FromSeconds(this.Duration));
            this.BeginAnimation(MarginProperty, ta);
        }
    }
}
