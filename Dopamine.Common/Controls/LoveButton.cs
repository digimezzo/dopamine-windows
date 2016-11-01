using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class LoveButton : Control
    {
        #region Properties
        public bool Love
        {
            get { return Convert.ToBoolean(GetValue(LoveProperty)); }
            set { SetValue(LoveProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty LoveProperty = DependencyProperty.Register("Love", typeof(bool), typeof(LoveButton), new PropertyMetadata(false));
        #endregion

        #region Construction
        static LoveButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoveButton), new FrameworkPropertyMetadata(typeof(LoveButton)));
        }
        #endregion
    }
}
