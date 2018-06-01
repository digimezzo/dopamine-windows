using Prism.Regions;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.FullPlayer
{
    public partial class FullPlayer : UserControl
    {
        private IRegionManager regionManager;

        public bool IsMouseOverSplitViewButton
        {
            get { return (bool)GetValue(IsMouseOverSplitViewButtonProperty); }
            set { SetValue(IsMouseOverSplitViewButtonProperty, value); }
        }

        public static readonly DependencyProperty IsMouseOverSplitViewButtonProperty =
            DependencyProperty.Register(nameof(IsMouseOverSplitViewButton), typeof(bool), typeof(FullPlayer), new PropertyMetadata(false));

        public FullPlayer(IRegionManager regionManager)
        {
            InitializeComponent();

            this.regionManager = regionManager;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            RegionManager.SetRegionManager(this.SplitViewContent, this.regionManager);
            RegionManager.UpdateRegions();
        }

        private void MySplitView_PaneOpened(object sender, System.EventArgs e)
        {
            this.MySplitView.ShowButton = false;
        }

        private void MySplitView_PaneClosed(object sender, System.EventArgs e)
        {
            this.MySplitView.ShowButton = true;
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.IsMouseOverSplitViewButton = true;
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.IsMouseOverSplitViewButton = false;
        }
    }
}
