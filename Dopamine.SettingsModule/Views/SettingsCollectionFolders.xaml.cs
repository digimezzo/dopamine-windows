using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.SettingsModule.Views
{
    public partial class SettingsCollectionFolders : UserControl, IView
    {
        #region Properties
        public bool ShowControls
        {
            get { return Convert.ToBoolean(GetValue(ShowControlsProperty)); }

            set { SetValue(ShowControlsProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ShowControlsProperty = DependencyProperty.Register("ShowControls", typeof(bool), typeof(SettingsCollectionFolders), new PropertyMetadata(null));
        #endregion

        #region Construction
        public SettingsCollectionFolders()
        {
            InitializeComponent();
        }
        #endregion
    }
}
