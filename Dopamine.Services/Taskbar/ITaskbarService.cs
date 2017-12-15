using System.Windows.Media;
using System.Windows.Shell;

namespace Dopamine.Services.Taskbar
{
    public interface ITaskbarService
    {
        double ProgressValue { get; }
        TaskbarItemProgressState ProgressState { get; }
        string Description { get; }
        string PlayPauseText { get; }
        ImageSource PlayPauseIcon { get; }
        void SetShowProgressInTaskbar(bool showProgressInTaskbar);
    }
}
