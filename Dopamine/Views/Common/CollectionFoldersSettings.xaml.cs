using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class CollectionFoldersSettings : UserControl
    {
        public bool ShowControls
        {
            get { return Convert.ToBoolean(GetValue(ShowControlsProperty)); }
            set { SetValue(ShowControlsProperty, value); }
        }

        public static readonly DependencyProperty ShowControlsProperty = 
            DependencyProperty.Register(nameof(ShowControls), typeof(bool), typeof(CollectionFoldersSettings), new PropertyMetadata(null));

        public CollectionFoldersSettings()
        {
            InitializeComponent();
        }
    }
}
