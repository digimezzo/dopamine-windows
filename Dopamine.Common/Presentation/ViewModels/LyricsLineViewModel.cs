using Prism.Mvvm;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsLineViewModel : BindableBase
    {
        #region Variables
        private TimeSpan time;
        private string text;
        private bool isActive;
        #endregion

        #region Properties
        public TimeSpan Time
        {
            get { return this.time; }
        }

        public string Text
        {
            get { return this.text; }
        }

        public bool IsActive
        {
            get { return this.isActive; }
            set { SetProperty<bool>(ref this.isActive, value); }
        }
        #endregion

        #region Construction
        public LyricsLineViewModel(TimeSpan time, string text)
        {
            this.time = time;
            this.text = text;
        }
        #endregion
    }
}
