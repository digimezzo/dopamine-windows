using Prism.Mvvm;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Playlists
{
    public class PlaylistsSmartPlaylistsCreateViewModel : BindableBase
    {
        public async Task<bool> SaveSmartPlaylistAsync()
        {
            await Task.Delay(20);
            return true;
        }
    }
}
