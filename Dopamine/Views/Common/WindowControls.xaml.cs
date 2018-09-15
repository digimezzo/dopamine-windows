using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class WindowControls : UserControl
    {
        public double ButtonWidth
        {
            get { return Convert.ToDouble(GetValue(ButtonWidthProperty)); }

            set { SetValue(ButtonWidthProperty, value); }
        }

        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register(nameof(ButtonWidth), typeof(double), typeof(WindowControls), new PropertyMetadata(null));

        public double ButtonHeight
        {
            get { return Convert.ToDouble(GetValue(ButtonHeightProperty)); }

            set { SetValue(ButtonHeightProperty, value); }
        }

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(WindowControls), new PropertyMetadata(null));

        public bool ShowMaximizeRestoreButton
        {
            get { return Convert.ToBoolean(GetValue(ShowMaximizeRestoreButtonProperty)); }

            set { SetValue(ShowMaximizeRestoreButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowMaximizeRestoreButtonProperty =
            DependencyProperty.Register(nameof(ShowMaximizeRestoreButton), typeof(bool), typeof(WindowControls), new PropertyMetadata(false));

        public WindowControls()
        {
            InitializeComponent();
        }
    }
}
