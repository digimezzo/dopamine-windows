using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsViewModel : BindableBase
    {
        #region Properties
        private string title;
        private string staticLyrics;
        #endregion

        #region Properties
        public string Title
        {
            get { return this.title; }
            set { SetProperty<string>(ref this.title, value); }
        }

        public string StaticLyrics
        {
            get { return this.staticLyrics; }
            set { SetProperty<string>(ref this.staticLyrics, value); }
        }
        #endregion
    }
}
