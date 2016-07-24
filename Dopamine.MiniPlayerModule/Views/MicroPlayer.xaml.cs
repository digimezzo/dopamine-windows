using System.Windows.Input;

namespace Dopamine.MiniPlayerModule.Views
{
    public partial class MicroPlayer : CommonMiniPlayerView
    {
        #region Construction
        public MicroPlayer()
        {
            InitializeComponent();
        }
        #endregion

        #region Private
        private void CoverGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.MouseLeftButtonDownHandler(sender, e);
        }
        #endregion
    }
}
