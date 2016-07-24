using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class BackgroundCoverArtControl : UserControl, IView
    {
        #region Construction
        public BackgroundCoverArtControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
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
        #endregion
    }
}
