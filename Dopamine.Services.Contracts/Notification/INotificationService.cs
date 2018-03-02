using Digimezzo.WPFControls;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Services.Contracts.Notification
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
        void SetApplicationWindows(BorderlessWindows10Window mainWindow, BorderlessWindows10Window playlistWindow, Window trayControlsWindow);
    }
}
