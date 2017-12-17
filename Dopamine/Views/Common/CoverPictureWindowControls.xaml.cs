using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class CoverPictureWindowControls : UserControl
    {
        public double ButtonWidth
        {
            get { return Convert.ToDouble(GetValue(ButtonWidthProperty)); }

            set { SetValue(ButtonWidthProperty, value); }
        }

        public double ButtonHeight
        {
            get { return Convert.ToDouble(GetValue(ButtonHeightProperty)); }

            set { SetValue(ButtonHeightProperty, value); }
        }
 
        public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(double), typeof(CoverPictureWindowControls), new PropertyMetadata(null));
        public static readonly DependencyProperty ButtonHeightProperty = DependencyProperty.Register("ButtonHeight", typeof(double), typeof(CoverPictureWindowControls), new PropertyMetadata(null));
 
        public CoverPictureWindowControls()
        {
            InitializeComponent();
        }
    }
}
