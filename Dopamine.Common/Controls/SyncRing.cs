using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class SyncRing : Label
    {
        #region Middle
        public double Middle
        {
            get { return Convert.ToDouble(GetValue(MiddleProperty)); }

            set { SetValue(MiddleProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty MiddleProperty = DependencyProperty.Register("Middle", typeof(double), typeof(SyncRing), new PropertyMetadata(null));
        #endregion


        #region Construction
        static SyncRing()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SyncRing), new FrameworkPropertyMetadata(typeof(SyncRing)));
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.SizeChanged += SizeChangedHandler;
        }
        #endregion

        #region Event Handlers
        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            this.Middle = this.Width / 2;
        }
        #endregion
    }
}
