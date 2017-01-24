using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Api.Lyrics;
using Dopamine.Common.Database;
using Dopamine.Common.Metadata;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using System.Linq;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class LyricsViewModel : CommonViewModel
    {
        #region Variables
        private PlayableTrack track;
        private Lyrics lyrics;
        private Lyrics uneditedLyrics;
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
        public DelegateCommand SaveIfNotEmptyCommand { get; set; }
        #endregion

        #region Properties
        public bool HasSource
        {
            get { return this.lyrics != null ? this.lyrics.HasSource : false; }
        }

        public bool ShowSource
        {
            get { return this.lyrics != null ? this.lyrics.HasText : false; }
        }

        public bool IsNoLyricsTextVisible
        {
            get { return (this.lyrics == null || string.IsNullOrEmpty(this.lyrics.Text)) & !this.IsEditing; }
        }

        public bool IsEditing
        {
            get { return this.isEditing; }
            set
            {
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
                SettingsClient.Set<int>("Lyrics", "FontSize", (int)value);
                OnPropertyChanged(() => this.FontSizePixels);
            }
        }

        public bool AutomaticScrolling
        {
            get { return this.automaticScrolling; }
            set
            {
                SetProperty<bool>(ref this.automaticScrolling, value);
                SettingsClient.Set<bool>("Lyrics", "AutomaticScrolling", value);
            }
        }

        public string FontSizePixels
        {
            get { return this.fontSize.ToString() + " px"; }
        }

        public Lyrics Lyrics
        {
            get { return this.lyrics; }
            set
            {
                SetProperty<Lyrics>(ref this.lyrics, value);
                OnPropertyChanged(() => this.IsNoLyricsTextVisible);
                OnPropertyChanged(() => this.ShowSource);
                OnPropertyChanged(() => this.HasSource);
            }
        }

        public ObservableCollection<LyricsLineViewModel> LyricsLines
        {
            get { return this.lyricsLines; }
        }
        #endregion

        #region Construction
        public LyricsViewModel(IUnityContainer container, PlayableTrack track, IMetadataService metadataService) : base(container)
        {
            this.track = track;
            this.metadataService = metadataService;

            this.FontSize = SettingsClient.Get<double>("Lyrics", "FontSize");
            this.AutomaticScrolling = SettingsClient.Get<bool>("Lyrics", "AutomaticScrolling");

            this.DecreaseFontSizeCommand = new DelegateCommand(() => { if (this.FontSize > 11) this.FontSize--; });
            this.IncreaseFontSizeCommand = new DelegateCommand(() => { if (this.FontSize < 50) this.FontSize++; });
            this.EditCommand = new DelegateCommand(() => { this.IsEditing = true; });
            this.CancelEditCommand = new DelegateCommand(() =>
            {
                this.lyrics = this.uneditedLyrics;
                this.IsEditing = false;
            });

            this.SaveCommand = new DelegateCommand(async () => await this.SaveLyricsInAudioFileAsync());
            this.SaveIfNotEmptyCommand = new DelegateCommand(async () => await this.SaveLyricsInAudioFileAsync(), () => !string.IsNullOrWhiteSpace(this.lyrics.Text));

            this.SearchOnlineCommand = new DelegateCommand<string>((id) =>
            {
                this.PerformSearchOnline(id, this.track.ArtistName, this.track.TrackTitle);
            });
        }

        private async Task SaveLyricsInAudioFileAsync()
        {
            this.IsEditing = false;
            this.ParseLyrics(this.lyrics);

            // Save to the file
            var fmd = await this.metadataService.GetFileMetadataAsync(this.track.Path);
            fmd.Lyrics = new MetadataValue() { Value = this.lyrics.Text };
            var fmdList = new List<FileMetadata>();
            fmdList.Add(fmd);
            await this.metadataService.UpdateTracksAsync(fmdList, false);
        }

        public LyricsViewModel(IUnityContainer container) : base(container)
        {
        }
        #endregion

        #region Public
        public void SetLyrics(Lyrics lyrics)
        {
            this.Lyrics = lyrics;
            this.uneditedLyrics = lyrics;
            this.ParseLyrics(lyrics);
        }
        #endregion

        #region Private
        private void ParseLyrics(Lyrics lyrics)
        {
            Application.Current.Dispatcher.Invoke(() => this.lyricsLines = null);
            var localLyricsLines = new ObservableCollection<LyricsLineViewModel>();
            var reader = new StringReader(lyrics.Text);
            string line;

            while (true)
            {
                line = reader.ReadLine();
                if (line != null)
                {
                    if (line.Length > 0 && line.StartsWith("["))
                    {
                        int index = line.LastIndexOf(']');

                        var time = TimeSpan.Zero;

                        // -1 means: not found (We check for > 0, because > -1 makes no sense in this case)
                        if (index > 0)
                        {
                            MatchCollection ms = Regex.Matches(line, @"\[.*?\]");
                            foreach (Match m in ms)
                            {
                                var subString = m.Value.Trim('[', ']');
                                if (TimeSpan.TryParseExact(subString, new string[] { @"mm\:ss\.fff", @"mm\:ss\.ff", @"mm\:ss" }, System.Globalization.CultureInfo.InvariantCulture, out time))
                                {
                                    localLyricsLines.Add(new LyricsLineViewModel(time, line.Substring(index + 1)));
                                }
                                else
                                {
                                    // The string between square brackets could not be parsed to a timestamp.
                                    // In such case, just add the complete lyrics line.
                                    localLyricsLines.Add(new LyricsLineViewModel(line));
                                }
                            }
                        }
                    }
                    else
                    {
                        localLyricsLines.Add(new LyricsLineViewModel(line));
                    }
                }
                else
                {
                    break;
                }
            }

            localLyricsLines = new ObservableCollection<LyricsLineViewModel>(localLyricsLines.OrderBy(p => p.Time));

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.lyricsLines = localLyricsLines;
                OnPropertyChanged(() => this.LyricsLines);
            });
        }

        protected async override Task LoadedCommandAsync()
        {
           // Not required here
        }
        #endregion
    }
}
