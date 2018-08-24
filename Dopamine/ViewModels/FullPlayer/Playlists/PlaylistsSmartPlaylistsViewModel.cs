using Dopamine.ViewModels.Common.Base;
using GongSolutions.Wpf.DragDrop;
using Prism.Ioc;

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsSmartPlaylistsViewModel : TracksViewModelBaseWithTrackArt, IDropTarget
    {
        public PlaylistsSmartPlaylistsViewModel(IContainerProvider container) : base(container)
        {
        }

        public void DragOver(IDropInfo dropInfo)
        {
            throw new System.NotImplementedException();
        }

        public void Drop(IDropInfo dropInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}
