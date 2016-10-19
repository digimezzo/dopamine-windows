using Prism.Mvvm;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsLineViewModel : BindableBase
    {
        #region Variables
        private TimeSpan time;
        private string text;
        private bool isHighlighted;
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

        public bool IsHighlighted
        {
            get { return this.isHighlighted; }
            set { SetProperty<bool>(ref this.isHighlighted, value); }
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
