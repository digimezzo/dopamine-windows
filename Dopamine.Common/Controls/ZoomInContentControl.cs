using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Dopamine.Common.Controls
{
    public class ZoomInContentControl : ContentControl
    {
        #region Properties
        public double Duration
        {
            get { return Convert.ToDouble(GetValue(DurationProperty)); }

            set { SetValue(DurationProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(double), typeof(ZoomInContentControl), new PropertyMetadata(0.5));
        #endregion

        #region Overrides
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (newContent != null)
            {
                this.DoAnimation();
            }
        }
        #endregion

        #region Private
        private void DoAnimation()
        {
            var ta = new ThicknessAnimation();
            ta.From = new Thickness(this.ActualWidth / 2, this.ActualHeight / 2, this.ActualWidth / 2, this.ActualHeight / 2);
            ta.To = new Thickness(0, 0, 0, 0);
            ta.Duration = new Duration(TimeSpan.FromSeconds(this.Duration));
            this.BeginAnimation(MarginProperty, ta);
        }
        #endregion
    }
}
