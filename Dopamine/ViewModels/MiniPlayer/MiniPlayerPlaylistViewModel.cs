using Prism.Mvvm;

namespace Dopamine.ViewModels.MiniPlayer
{
    public class MiniPlayerPlaylistViewModel : BindableBase
    {
        private int nowPlayingSelectedPageIndex;

        public int NowPlayingSelectedPageIndex
        {
            get { return nowPlayingSelectedPageIndex; }
            set { SetProperty<int>(ref this.nowPlayingSelectedPageIndex, value); }
        }
    }
}
