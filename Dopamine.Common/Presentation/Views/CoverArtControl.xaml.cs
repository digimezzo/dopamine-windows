using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    /// <summary>
    /// Interaction logic for CoverArtControl.xaml
    /// </summary>
    public partial class CoverArtControl : UserControl, IView
    {
        #region Dependency Properties
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(CoverArtControl), new PropertyMetadata(null));
        #endregion
    
        #region Properties
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
        #endregion

        #region Construction
        public CoverArtControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Private
        private void ThisControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.IconSize = Convert.ToDouble(Convert.ToInt32(this.ActualWidth / 2)); // We want this to be a rounded value

        }
        #endregion

        #region Event Handlers
        private void ThisControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.IconSize = Convert.ToDouble(Convert.ToInt32(this.ActualWidth / 2)); // We want this to be a rounded value
        }
        #endregion
    }
}
