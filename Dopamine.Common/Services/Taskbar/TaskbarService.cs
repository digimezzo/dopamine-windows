using System.ComponentModel;
using System.Windows.Shell;

namespace Dopamine.Common.Services.Taskbar
{
    public class TaskbarService : ITaskbarService, INotifyPropertyChanged
    {
        #region Variables
        private string description;
        private TaskbarItemProgressState progressState;
        private double progressValue;
        #endregion

        #region Properties
        public string Description
        {
            get { return this.description; }
            set
            {
                this.description = value;
                OnPropertyChanged("Description");
            }
        }

        public TaskbarItemProgressState ProgressState
        {
            get { return this.progressState; }
            set
            {
                this.progressState = value;
                OnPropertyChanged("ProgressState");
            }
        }

        public double ProgressValue
        {
            get { return this.progressValue; }
            set
            {
                this.progressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }
        #endregion

        #region ITaskbarService
        public void SetTaskbarProgressState(bool showProgressInTaskbar, bool isPlaying)
        {
            if (showProgressInTaskbar)
            {
                if (isPlaying)
                {
                    this.ProgressState = TaskbarItemProgressState.Normal;
                }
                else
                {
                    this.ProgressState = TaskbarItemProgressState.Paused;
                }
            }
            else
            {
                this.ProgressValue = 0;
                this.ProgressState = TaskbarItemProgressState.None;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string name)
        {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
