using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsViewModel : BindableBase
    {
        #region Properties
        private string title;
        private string lyrics;
        private ObservableCollection<TimeStampedLyricsLineViewModel> timeStampedLyrics;
        #endregion

        #region Properties
        public bool HasTimeStampedLyrics
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
            get { return string.IsNullOrEmpty(this.lyrics) ? string.Empty : this.lyrics; }
        }

        public ObservableCollection<TimeStampedLyricsLineViewModel> TimeStampedLyrics
        {
            get { return this.timeStampedLyrics; }
        }
        #endregion

        #region Construction
        public LyricsViewModel(string title)
        {
            this.Title = title;
        }
        #endregion

        #region Public
        public async Task SetLyricsAsync(string lyrics)
        {
            this.lyrics = lyrics;

            await this.ParseTimeStampedLyricsAsync(lyrics);

            OnPropertyChanged(() => this.Lyrics);
            OnPropertyChanged(() => this.TimeStampedLyrics);
            OnPropertyChanged(() => this.HasTimeStampedLyrics);
        }
        #endregion

        #region Private
        private async Task ParseTimeStampedLyricsAsync(string lyrics)
        {
            var previousTime = new TimeSpan(0);

            await Task.Run(() =>
            {
                this.timeStampedLyrics = new ObservableCollection<TimeStampedLyricsLineViewModel>();
                var reader = new StringReader(lyrics);
                string line;

                while (true)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (line.Length > 0 && line.StartsWith("["))
                        {
                            int index = line.IndexOf(']');

                            var time = new TimeSpan(0);

                            // -1 means: not found (We check for > 0, because > -1 makes no sense in this case)
                            if (index > 0)
                            {
                                var subString = line.Substring(1, index-1);
                                if (TimeSpan.TryParseExact(subString, new string[] { @"mm\:ss\.fff", @"mm\:ss" }, System.Globalization.CultureInfo.InvariantCulture, out time))
                                {
                                    this.timeStampedLyrics.Add(new TimeStampedLyricsLineViewModel(time, line.Substring(index + 1)));
                                    previousTime = time;
                                }
                            }
                        }
                        else
                        {
                            this.timeStampedLyrics.Add(new TimeStampedLyricsLineViewModel(previousTime, line));
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }
        #endregion
    }
}
