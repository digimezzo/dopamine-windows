using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class CollectionFolders : UserControl
    {
        public bool ShowControls
        {
            get { return Convert.ToBoolean(GetValue(ShowControlsProperty)); }
            set { SetValue(ShowControlsProperty, value); }
        }

        public static readonly DependencyProperty ShowControlsProperty = 
            DependencyProperty.Register(nameof(ShowControls), typeof(bool), typeof(CollectionFolders), new PropertyMetadata(null));

        public CollectionFolders()
        {
            InitializeComponent();
        }
    }
}
