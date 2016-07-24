using Microsoft.Practices.Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackInfoViewModel : BindableBase
    {
        #region Variables
        private string title;
        private string artist;
        private string album;
        private string year;
        private string currentTime;
        private string totalTime;
        #endregion

        #region Properties
        public string Title
        {
            get { return this.title; }
            set { SetProperty<string>(ref this.title, value); }
        }

        public string Artist
        {
            get { return this.artist; }
            set { SetProperty<string>(ref this.artist, value); }
        }

        public string Album
        {
            get { return this.album; }
            set { SetProperty<string>(ref this.album, value); }
        }

        public string Year
        {
            get { return this.year; }
            set { SetProperty<string>(ref this.year, value); }
        }

        public string CurrentTime
        {
            get { return currentTime; }
            set { SetProperty<string>(ref this.currentTime, value); }
        }

        public string TotalTime
        {
            get { return this.totalTime; }
            set { SetProperty<string>(ref this.totalTime, value); }
        }
        #endregion
    }
}
