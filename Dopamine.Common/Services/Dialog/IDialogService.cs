using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dopamine.Common.Services.Dialog
{
    public interface IDialogService
    {   
        bool ShowConfirmation(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText);
        bool ShowNotification(int iconCharCode, int iconSize, string title, string content, string okText, bool showViewLogs, string viewLogsText = "Log file");
        bool ShowCustomDialog(int iconCharCode, int iconSize, string title, UserControl content, int width, int height, bool canResize, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback);
        bool ShowCustomDialog(UserControl icon, string title, UserControl content, int width, int height, bool canResize, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback);
        bool ShowInputDialog(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText, ref string responeText);
        event Action<bool> DialogVisibleChanged;
    }
}
