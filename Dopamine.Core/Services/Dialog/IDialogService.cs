using System;
using System.Threading.Tasks;

namespace Dopamine.Core.Services.Dialog
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText);
        bool ShowNotification(int iconCharCode, int iconSize, string title, string content, string okText, bool showViewLogs, string viewLogsText = "Log file");
        bool ShowCustomDialog(int iconCharCode, int iconSize, string title, object content, int width, int height, bool canResize, bool autoSize, bool showTitle, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback);
        bool ShowCustomDialog(object icon, string title, object content, int width, int height, bool canResize, bool autoSize, bool showTitle, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback);
        bool ShowInputDialog(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText, ref string responeText);
        event Action<bool> DialogVisibleChanged;
    }
}
