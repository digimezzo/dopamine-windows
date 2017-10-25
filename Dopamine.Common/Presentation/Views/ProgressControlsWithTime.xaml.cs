using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class ProgressControlsWithTime : UserControl
    {
        public static readonly DependencyProperty SliderLengthProperty = DependencyProperty.Register("SliderLength", typeof(double), typeof(ProgressControlsWithTime), new PropertyMetadata(100.0));
    
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
    
        public ProgressControlsWithTime()
        {
            InitializeComponent();
        }
    }
}
