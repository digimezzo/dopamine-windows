using Prism.Mvvm;

namespace Dopamine.ViewModels.NowPlaying
{
    public class NowPlayingViewModel : BindableBase
    {
        private int nowPlayingSelectedPageIndex;

        public int NowPlayingSelectedPageIndex
        {
            get { return nowPlayingSelectedPageIndex; }
            set { SetProperty<int>(ref this.nowPlayingSelectedPageIndex, value); }
        }
    }
}
