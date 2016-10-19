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
        private ObservableCollection<LyricsLineViewModel> lyricsLines;
        #endregion

        #region Properties
        public string Title
        {
            get { return this.title; }
            set { SetProperty<string>(ref this.title, value); }
        }

        public ObservableCollection<LyricsLineViewModel> LyricsLines
        {
            get { return this.lyricsLines; }
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
            await this.ParseLyricsAsync(lyrics);
            OnPropertyChanged(() => this.LyricsLines);
        }
        #endregion

        #region Private
        private async Task ParseLyricsAsync(string lyrics)
        {
            var previousTime = TimeSpan.Zero;

            await Task.Run(() =>
            {
                this.lyricsLines = new ObservableCollection<LyricsLineViewModel>();
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

                            var time = TimeSpan.Zero;

                            // -1 means: not found (We check for > 0, because > -1 makes no sense in this case)
                            if (index > 0)
                            {
                                var subString = line.Substring(1, index-1);
                                if (TimeSpan.TryParseExact(subString, new string[] { @"mm\:ss\.fff", @"mm\:ss" }, System.Globalization.CultureInfo.InvariantCulture, out time))
                                {
                                    this.lyricsLines.Add(new LyricsLineViewModel(time, line.Substring(index + 1)));
                                    previousTime = time;
                                }
                            }
                        }
                        else
                        {
                            this.lyricsLines.Add(new LyricsLineViewModel(previousTime, line));
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
