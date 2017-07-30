using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Common.Services.Appearance
{
    public interface IAppearanceService : Core.Services.Appearance.IAppearanceService
    {
        Task ApplyColorScheme(bool followWindowsColor, bool followAlbumCoverColor, bool isViewModelLoaded = false, string selectedColorScheme = "");
        void WatchWindowsColor(Window window);
    }
}
