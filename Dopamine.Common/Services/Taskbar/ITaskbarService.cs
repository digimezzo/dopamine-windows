using System.Windows.Shell;

namespace Dopamine.Common.Services.Taskbar
{
    public interface ITaskbarService
    {
        double ProgressValue { get; set; }
        TaskbarItemProgressState ProgressState { get; set; }
        string Description { get; set; }
        void SetTaskbarProgressState(bool showProgressInTaskbar, bool isPaused);
    }
}
