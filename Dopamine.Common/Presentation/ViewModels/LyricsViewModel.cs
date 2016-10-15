using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsViewModel : BindableBase
    {
        #region Properties
        private string title;
        private string lyrics;
        private ObservableCollection<Dictionary<DateTime, string>> timeStampedLyrics;
        #endregion

        #region Properties
        public bool HasTimeStampdLyrics
        {
            get { return this.timeStampedLyrics != null && this.timeStampedLyrics.Count > 0; }
        }

        public string Title
        {
            get { return this.title; }
            set { SetProperty<string>(ref this.title, value); }
        }

        public string Lyrics
        {
            get { return this.lyrics; }
            set { SetProperty<string>(ref this.lyrics, value); }
        }

        public ObservableCollection<Dictionary<DateTime, string>> TimestampedLyrics
        {
            get { return this.timeStampedLyrics; }
            set {
                SetProperty<ObservableCollection<Dictionary<DateTime, string>>>(ref this.timeStampedLyrics, value);
                OnPropertyChanged(() => this.HasTimeStampdLyrics);
            }
        }

        #endregion
    }
}
