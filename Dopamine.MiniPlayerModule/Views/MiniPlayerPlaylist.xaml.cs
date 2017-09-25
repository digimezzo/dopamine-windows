using Prism.Events;
using System.Windows.Controls;

namespace Dopamine.MiniPlayerModule.Views
{
    public partial class MiniPlayerPlaylist : UserControl
    {
        #region Variable
        private SubscriptionToken scrollToPlayingTrackToken;
        #endregion

        #region Construction
        public MiniPlayerPlaylist() : base()
        {
            InitializeComponent();
        }
        #endregion
    }
}
