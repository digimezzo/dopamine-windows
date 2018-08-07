using Digimezzo.Foundation.WPF.Controls;
using Prism.Regions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Dopamine.Views.FullPlayer
{
    public partial class FullPlayer : UserControl
    {
        private IRegionManager regionManager;

        public FullPlayer(IRegionManager regionManager)
        {
            InitializeComponent();

            this.regionManager = regionManager;
        }

        private void AnimateHamburgerIcon(int newSize, int newOpacity, TimeSpan duration)
        {
            DoubleAnimation sizeAnimation = new DoubleAnimation(newSize, duration);
            DoubleAnimation opacityAnimation = new DoubleAnimation(newOpacity, duration);

            this.HamburgerIcon.BeginAnimation(MaterialIcon.FontSizeProperty, sizeAnimation);
            this.HamburgerIcon.BeginAnimation(MaterialIcon.OpacityProperty, opacityAnimation);
        }

        private void AnimateHeadPhoneIcon(int newSize, int newOpacity, TimeSpan duration)
        {
            DoubleAnimation sizeAnimation = new DoubleAnimation(newSize, duration);
            DoubleAnimation opacityAnimation = new DoubleAnimation(newOpacity, duration);

            this.HeadPhoneIcon.BeginAnimation(MaterialIcon.WidthProperty, sizeAnimation);
            this.HeadPhoneIcon.BeginAnimation(MaterialIcon.HeightProperty, sizeAnimation);
            this.HeadPhoneIcon.BeginAnimation(MaterialIcon.OpacityProperty, opacityAnimation);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            RegionManager.SetRegionManager(this.SplitViewContent, this.regionManager);
            RegionManager.UpdateRegions();
            this.CollectionButton.IsChecked = true;
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
            this.AnimateHamburgerIcon(24, 1, TimeSpan.FromMilliseconds(250));
            this.AnimateHeadPhoneIcon(1, 0, TimeSpan.FromMilliseconds(250));
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.AnimateHamburgerIcon(1, 0, TimeSpan.FromMilliseconds(250));
            this.AnimateHeadPhoneIcon(24, 1, TimeSpan.FromMilliseconds(250));
        }

        private void InformationButton_Checked(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
        }

        private void CollectionButton_Checked(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
        }

        private void SettingsButton_Checked(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
        }

        private void HeaderButton_Click(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
        }
    }
}
