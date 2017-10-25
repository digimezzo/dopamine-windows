using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class CoverArtControl : UserControl
    {
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(CoverArtControl), new PropertyMetadata(null));
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }

        public double IconSize
        {
            get { return Convert.ToDouble(GetValue(IconSizeProperty)); }

            set { SetValue(IconSizeProperty, value); }
        }
     
        public CoverArtControl()
        {
            InitializeComponent();
        }
    
        private void ThisControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.IconSize = Convert.ToDouble(Convert.ToInt32(this.ActualWidth / 2)); // We want this to be a rounded value

        }
  
        private void ThisControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.IconSize = Convert.ToDouble(Convert.ToInt32(this.ActualWidth / 2)); // We want this to be a rounded value
        }
    }
}
