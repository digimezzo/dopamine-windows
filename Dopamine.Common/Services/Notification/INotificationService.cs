using Dopamine.Common.Controls;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Common.Services.Notification
{
    public interface INotificationService
    {
        bool CanShowNotification { get; }
        Task ShowNotificationAsync();
        void HideNotification();
        void SetApplicationWindows(DopamineWindow mainWindow, DopamineWindow playlistWindow, Window trayControlsWindow);
    }
}
