using Dopamine.Common.Services.Metadata;
using Dopamine.Core.Metadata;
using Dopamine.Core.Settings;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsViewModel : BindableBase
    {
        #region Variables
        private string filename;
        private string lyrics;
        private string uneditedLyrics;
        private ObservableCollection<LyricsLineViewModel> lyricsLines;
        private double fontSize;
        private bool automaticScrolling;
        private bool isEditing;
        private IMetadataService metadataService;
        #endregion

        #region Commands
        public DelegateCommand DecreaseFontSizeCommand { get; set; }
        public DelegateCommand IncreaseFontSizeCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand CancelEditCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        #endregion

        #region Properties
        public bool IsNoLyricsTextVisible
        {
            get { return string.IsNullOrEmpty(this.lyrics) & !this.IsEditing; }
        }

        public bool IsEditing
        {
            get { return this.isEditing; }
            set {
                SetProperty<bool>(ref this.isEditing, value);
                OnPropertyChanged(() => this.IsNoLyricsTextVisible);
            }
        }

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

        public bool AutomaticScrolling
        {
            get { return this.automaticScrolling; }
            set
            {
                SetProperty<bool>(ref this.automaticScrolling, value);
                XmlSettingsClient.Instance.Set<bool>("Lyrics", "AutomaticScrolling", value);
            }
        }

        public string FontSizePixels
        {
            get { return this.fontSize.ToString() + " px"; }
        }

        public string Lyrics
        {
            get { return this.lyrics; }
            set
            {
                SetProperty<string>(ref this.lyrics, value);
                OnPropertyChanged(() => this.IsNoLyricsTextVisible);
            }
        }

        public ObservableCollection<LyricsLineViewModel> LyricsLines
        {
            get { return this.lyricsLines; }
        }
        #endregion

        #region Construction
        public LyricsViewModel(string filename, IMetadataService metadataService)
        {
            this.filename = filename;
            this.metadataService = metadataService;

            this.FontSize = XmlSettingsClient.Instance.Get<double>("Lyrics", "FontSize");
            this.AutomaticScrolling = XmlSettingsClient.Instance.Get<bool>("Lyrics", "AutomaticScrolling");

            this.DecreaseFontSizeCommand = new DelegateCommand(() => { if (this.FontSize > 11) this.FontSize--; });
            this.IncreaseFontSizeCommand = new DelegateCommand(() => { if (this.FontSize < 50) this.FontSize++; });
            this.EditCommand = new DelegateCommand(() => { this.IsEditing = true; });
            this.CancelEditCommand = new DelegateCommand(() =>
            {
                this.lyrics = this.uneditedLyrics;
                this.IsEditing = false;
            });

            this.SaveCommand = new DelegateCommand(() =>
            {
                this.IsEditing = false;
                this.ParseLyrics(this.lyrics);

                // Save to the file
                var fmd = new FileMetadata(this.filename);
                fmd.Lyrics = new MetadataValue() { Value = this.lyrics };
                var fmdList = new List<FileMetadata>();
                fmdList.Add(fmd);
                this.metadataService.UpdateTrackAsync(fmdList, false);
            });
        }

        public LyricsViewModel()
        {
        }
        #endregion

        #region Public
        public void SetLyrics(string lyrics)
        {
            this.Lyrics = lyrics;
            this.uneditedLyrics = lyrics;
            this.ParseLyrics(lyrics);
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

            OnPropertyChanged(() => this.LyricsLines);
        }
        #endregion
    }
}
