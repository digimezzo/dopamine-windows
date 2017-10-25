using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class BackgroundCoverArtControl : UserControl
    {
        public BackgroundCoverArtControl()
        {
            InitializeComponent();
        }
   
        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.ActualWidth == 0 | this.ActualHeight == 0) return;

            if (this.ActualWidth > this.ActualHeight)
            {
                this.CoverImage.Width = this.ActualWidth;
                this.CoverImage.Height = this.ActualWidth;
            }
            else
            {
                this.CoverImage.Width = this.ActualHeight;
                this.CoverImage.Height = this.ActualHeight;
            }
        }
    }
}
