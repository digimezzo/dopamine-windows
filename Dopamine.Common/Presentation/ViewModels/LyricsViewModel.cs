using Dopamine.Core.Settings;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsViewModel : BindableBase
    {
        #region Variables
        private ObservableCollection<LyricsLineViewModel> lyricsLines;
        private double fontSize;
        #endregion

        #region Commands
        public DelegateCommand DecreaseFontSizeCommand { get; set; }
        public DelegateCommand IncreaseFontSizeCommand { get; set; }
        #endregion

        #region Properties
        public double FontSize
        {
            get { return this.fontSize; }
            set
            {
                SetProperty<double>(ref this.fontSize, value);
                XmlSettingsClient.Instance.Set<int>("Lyrics", "FontSize", (int)value);
                OnPropertyChanged(() => this.FontSizePixels);
            }
        }

        public string FontSizePixels
        {
            get { return this.fontSize.ToString() + "px"; }
        }

        public ObservableCollection<LyricsLineViewModel> LyricsLines
        {
            get { return this.lyricsLines; }
        }
        #endregion

        #region Construction
        public LyricsViewModel()
        {
            this.FontSize = XmlSettingsClient.Instance.Get<double>("Lyrics", "FontSize");

            this.DecreaseFontSizeCommand = new DelegateCommand(() => { if (this.FontSize > 12) this.FontSize--; });
            this.IncreaseFontSizeCommand = new DelegateCommand(() => { if (this.FontSize < 50) this.FontSize++; });
        }
        #endregion

        #region Public
        public void SetLyrics(string lyrics)
        {
            this.ParseLyrics(lyrics);
            OnPropertyChanged(() => this.LyricsLines);
        }
        #endregion

        #region Private
        private void ParseLyrics(string lyrics)
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
                            var subString = line.Substring(1, index - 1);
                            if (TimeSpan.TryParseExact(subString, new string[] { @"mm\:ss\.fff", @"mm\:ss" }, System.Globalization.CultureInfo.InvariantCulture, out time))
                            {
                                this.lyricsLines.Add(new LyricsLineViewModel(time, line.Substring(index + 1)));
                            }
                        }
                    }
                    else
                    {
                        this.lyricsLines.Add(new LyricsLineViewModel(line));
                    }
                }
                else
                {
                    break;
                }
            }
        }
        #endregion
    }
}
