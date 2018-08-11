using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Controls;
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
        private SplitViewRadioButton goBackButton;

        public FullPlayer(IRegionManager regionManager)
        {
            InitializeComponent();

            this.regionManager = regionManager;

            this.MySplitView.ShowButton = false;
        }

        private void AnimateHamburgerIcon(int newSize, int newOpacity, TimeSpan duration)
        {
            DoubleAnimation sizeAnimation = new DoubleAnimation(newSize, duration);
            DoubleAnimation opacityAnimation = new DoubleAnimation(newOpacity, duration);

            this.HamburgerIcon.BeginAnimation(MaterialIcon.FontSizeProperty, sizeAnimation);
            this.HamburgerIcon.BeginAnimation(MaterialIcon.OpacityProperty, opacityAnimation);
        }

        private void AnimateBackIcon(int newSize, TimeSpan duration)
        {
            DoubleAnimation sizeAnimation = new DoubleAnimation(newSize, duration);

            this.BackIcon.BeginAnimation(SegoeIcon.FontSizeProperty, sizeAnimation);
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

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.AnimateHamburgerIcon(24, 1, TimeSpan.FromMilliseconds(250));
            this.AnimateHeadPhoneIcon(1, 0, TimeSpan.FromMilliseconds(250));
            this.AnimateBackIcon(20, TimeSpan.FromMilliseconds(125));
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.AnimateHamburgerIcon(1, 0, TimeSpan.FromMilliseconds(250));
            this.AnimateHeadPhoneIcon(24, 1, TimeSpan.FromMilliseconds(250));
            this.AnimateBackIcon(16, TimeSpan.FromMilliseconds(100));
        }

        private void InformationButton_Checked(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
        }

        private void CollectionButton_Checked(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
            this.goBackButton = (SplitViewRadioButton)sender;
        }

        private void PlaylistsButton_Checked(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
            this.goBackButton = (SplitViewRadioButton)sender;
        }

        private void SettingsButton_Checked(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
        }

        private void HeaderButton_Click(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = false;
        }

        private void OpenMenuButton_Click(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.goBackButton.IsChecked = true;
        }

        private void AlignSpectrumAnalyzer()
        {
            // This makes sure the spectrum analyzer is centered on the screen, based on the left pixel.
            // When we align center, alignment is sometimes (depending on the width of the screen) done
            // on a half pixel. This causes a blurry spectrum analyzer.
            try
            {
                this.SpectrumAnalyzer.Margin = new Thickness(Convert.ToInt32(this.ActualWidth / 2) - Convert.ToInt32(this.SpectrumAnalyzer.ActualWidth / 2), 0, 0, 0);
            }
            catch (Exception)
            {
                // Swallow this exception
            }
        }

        private void SpectrumAnalyzer_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.AlignSpectrumAnalyzer();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.AlignSpectrumAnalyzer();
        }
    }
}
