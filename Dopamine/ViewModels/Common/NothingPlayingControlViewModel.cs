using Dopamine.Services.Playback;
using GongSolutions.Wpf.DragDrop;
using Prism.Ioc;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.Common
{
    public class NothingPlayingControlViewModel : NowPlayingControlViewModel
    {
        public NothingPlayingControlViewModel(IContainerProvider container) : base(container)
        {
        }
    }
}
