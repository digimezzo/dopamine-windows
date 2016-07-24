using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class ProgressControlsWithTime : UserControl, IView
    {
        #region Dependency Properties
        public static readonly DependencyProperty SliderLengthProperty = DependencyProperty.Register("SliderLength", typeof(double), typeof(ProgressControlsWithTime), new PropertyMetadata(100.0));
        #endregion

        #region Properties
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }

        public double SliderLength
        {
            get { return Convert.ToDouble(GetValue(SliderLengthProperty)); }

            set { SetValue(SliderLengthProperty, value); }
        }
        #endregion

        #region Construction
        public ProgressControlsWithTime()
        {
            InitializeComponent();
        }
        #endregion
    }
}
