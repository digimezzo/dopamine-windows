using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class AppearanceTheme : UserControl
    {
        public bool ShowFollowAlbumCoverColor
        {
            get { return Convert.ToBoolean(GetValue(ShowFollowAlbumCoverColorProperty)); }
            set { SetValue(ShowFollowAlbumCoverColorProperty, value); }
        }

        public static readonly DependencyProperty ShowFollowAlbumCoverColorProperty =
            DependencyProperty.Register(nameof(ShowFollowAlbumCoverColor), typeof(bool), typeof(AppearanceTheme), new PropertyMetadata(null));

        public AppearanceTheme()
        {
            InitializeComponent();
        }
    }
}
