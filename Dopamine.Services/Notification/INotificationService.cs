using Digimezzo.Foundation.WPF.Controls;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Services.Notification
{
    public interface INotificationService
    {
        bool SupportsSystemNotification { get; }
        bool SystemNotificationIsEnabled { get; set; }
        bool ShowNotificationWhenPlaying { get; set; }
        bool ShowNotificationWhenPausing { get; set; }
        bool ShowNotificationWhenResuming { get; set; }
        bool ShowNotificationControls { get; set; }

        Task ShowNotificationAsync();
        void HideNotification();
        void SetApplicationWindows(Windows10BorderlessWindow mainWindow, Windows10BorderlessWindow playlistWindow, Window trayControlsWindow);
    }
}
