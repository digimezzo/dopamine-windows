using Digimezzo.Foundation.WPF.Controls;
using Prism.Regions;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Dopamine.Views.FullPlayer
{
    public partial class FullPlayer : UserControl
    {
        public FullPlayer()
        {
            InitializeComponent();
        }

        private void AnimateBackIcon(int newSize, TimeSpan duration)
        {
            DoubleAnimation sizeAnimation = new DoubleAnimation(newSize, duration);
            this.BackIcon.BeginAnimation(SegoeIcon.FontSizeProperty, sizeAnimation);
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.AnimateBackIcon(20, TimeSpan.FromMilliseconds(125));
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.AnimateBackIcon(16, TimeSpan.FromMilliseconds(100));
        }
    }
}
