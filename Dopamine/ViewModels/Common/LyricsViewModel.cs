using Digimezzo.Utilities.Settings;
using Dopamine.Core.Api.Lyrics;
using Dopamine.Core.Utils;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Data.Contracts.Metadata;
using Dopamine.Presentation.ViewModels;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Provider;
using Dopamine.ViewModels.Common.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Prism.Ioc;

namespace Dopamine.ViewModels.Common
{
    public class LyricsViewModel : ContextMenuViewModelBase
    {
        private PlayableTrack track;
        private Lyrics lyrics;
        private Lyrics uneditedLyrics;
        private ObservableCollection<LyricsLineViewModel> lyricsLines;
        private double fontSize;
        private bool automaticScrolling;
        private bool centerLyrics;
        private bool isEditing;
        private IMetadataService metadataService;
        private IProviderService providerService;

        public DelegateCommand DecreaseFontSizeCommand { get; set; }
        public DelegateCommand IncreaseFontSizeCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand CancelEditCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SaveIfNotEmptyCommand { get; set; }

        public bool HasSource
        {
            get { return this.lyrics != null ? this.lyrics.HasSource : false; }
        }

        public SourceTypeEnum? SourceType => this.lyrics?.SourceType;

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
                RaisePropertyChanged(nameof(this.IsNoLyricsTextVisible));
            }
        }

        public double FontSize
        {
            get { return this.fontSize; }
            set
            {
                SetProperty<double>(ref this.fontSize, value);
                SettingsClient.Set<int>("Lyrics", "FontSize", (int)value);
                RaisePropertyChanged(nameof(this.FontSizePixels));
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

        public bool CenterLyrics
        {
            get { return this.centerLyrics; }
            set
            {
                SetProperty<bool>(ref this.centerLyrics, value);
                SettingsClient.Set<bool>("Lyrics", "CenterLyrics", value);
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
                RaisePropertyChanged(nameof(this.IsNoLyricsTextVisible));
                RaisePropertyChanged(nameof(this.ShowSource));
                RaisePropertyChanged(nameof(this.HasSource));
            }
        }

        public ObservableCollection<LyricsLineViewModel> LyricsLines
        {
            get { return this.lyricsLines; }
        }

        public LyricsViewModel(IContainerProvider container, PlayableTrack track) : base(container)
        {
            this.track = track;

            // Dependency injection
            this.metadataService = container.Resolve<IMetadataService>();
            this.providerService = container.Resolve<IProviderService>();

            this.FontSize = SettingsClient.Get<double>("Lyrics", "FontSize");
            this.AutomaticScrolling = SettingsClient.Get<bool>("Lyrics", "AutomaticScrolling");
            this.CenterLyrics = SettingsClient.Get<bool>("Lyrics", "CenterLyrics");

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

            this.SearchOnlineCommand = new DelegateCommand<string>((id) => this.SearchOnline(id));
        }

        private async Task SaveLyricsInAudioFileAsync()
        {
            this.IsEditing = false;
            this.ParseLyrics(this.lyrics);

            // Save to the file
            var fmd = await this.metadataService.GetFileMetadataAsync(this.track.Path);
            fmd.Lyrics = new MetadataValue() { Value = this.lyrics.Text };

            var fmdList = new List<IFileMetadata>
            {
                fmd
            };

            await this.metadataService.UpdateTracksAsync(fmdList, false);
        }

        public LyricsViewModel(IContainerProvider container) : base(container)
        {
        }

        public void SetLyrics(Lyrics lyrics)
        {
            this.Lyrics = lyrics;
            this.uneditedLyrics = lyrics;
            this.ParseLyrics(lyrics);
        }

        private void ParseLyrics(Lyrics lyrics)
        {
            Application.Current.Dispatcher.Invoke(() => this.lyricsLines = null);
            var linesWithTimestamps = new ObservableCollection<LyricsLineViewModel>();
            var linesWithoutTimestamps = new ObservableCollection<LyricsLineViewModel>();
            var reader = new StringReader(lyrics.Text);
            string line;

            while (true)
            {
                // Process 1 line
                line = reader.ReadLine();

                if (line == null)
                {
                    // No line found, we reached the end. Exit while loop.
                    break;
                }

                // Check if the line has characters and is enclosed in brackets (starts with [ and ends with ]).
                if (line.Length == 0 || !(line.StartsWith("[") && line.LastIndexOf(']') > 0))
                {
                    // This line is not enclosed in brackets, so it cannot have timestamps.
                    linesWithoutTimestamps.Add(new LyricsLineViewModel(line));

                    // Process the next line
                    continue; 
                }

                // Check if the line is a tag
                MatchCollection tagMatches = Regex.Matches(line, @"\[[a-z]+?:.*?\]");

                if (tagMatches.Count > 0)
                {
                    // This is a tag: ignore this line and process the next line.
                    continue;
                }

                // Get all substrings between square brackets for this line
                MatchCollection ms = Regex.Matches(line, @"\[.*?\]");
                var timestamps = new List<TimeSpan>();
                bool couldParseAllTimestamps = true;

                // Loop through all matches
                foreach (Match m in ms)
                {
                    var time = TimeSpan.Zero;
                    string subString = m.Value.Trim('[', ']');

                    if (FormatUtils.ParseLyricsTime(subString, out time))
                    {
                        timestamps.Add(time);
                    }
                    else
                    {
                        couldParseAllTimestamps = false;
                    }
                }

                // Check if all timestamps could be parsed
                if (couldParseAllTimestamps)
                {
                    int startIndex = line.LastIndexOf(']') + 1;

                    foreach (TimeSpan timestamp in timestamps)
                    {
                        linesWithTimestamps.Add(new LyricsLineViewModel(timestamp, line.Substring(startIndex)));
                    }
                }
                else
                {
                    // The line has mistakes. Consider it as a line without timestamps.
                    linesWithoutTimestamps.Add(new LyricsLineViewModel(line));
                }
            }

            // Order the time stamped lines
            linesWithTimestamps = new ObservableCollection<LyricsLineViewModel>(linesWithTimestamps.OrderBy(p => p.Time));

            // Merge both collections, lines with timestamps first.
            linesWithTimestamps.AddRange(linesWithoutTimestamps);

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.lyricsLines = linesWithTimestamps;
                RaisePropertyChanged(nameof(this.LyricsLines));
            });
        }

        protected override void SearchOnline(string id)
        {
            this.providerService.SearchOnline(id, new string[] { this.track.ArtistName, this.track.TrackTitle });
        }
    }
}