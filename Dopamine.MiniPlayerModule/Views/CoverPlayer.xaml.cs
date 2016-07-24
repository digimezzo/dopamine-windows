using System.Windows.Input;

namespace Dopamine.MiniPlayerModule.Views
{
    public partial class CoverPlayer : CommonMiniPlayerView
    {
        #region Construction
        public CoverPlayer()
        {
            InitializeComponent();
        }
        #endregion

        #region Private
        protected void CoverGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.MouseLeftButtonDownHandler(sender, e);
        }
        #endregion
    }
}
