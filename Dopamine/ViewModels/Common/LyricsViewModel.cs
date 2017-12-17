using Digimezzo.Utilities.Settings;
using Dopamine.ViewModels.Common.Base;
using Dopamine.Core.Api.Lyrics;
using Dopamine.Core.Utils;
using Dopamine.Data;
using Dopamine.Data.Metadata;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Provider;
using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

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

        public LyricsViewModel(IUnityContainer container, PlayableTrack track) : base(container)
        {
            this.track = track;

            // Dependency injection
            this.metadataService = container.Resolve<IMetadataService>();
            this.providerService = container.Resolve<IProviderService>();

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

            this.SearchOnlineCommand = new DelegateCommand<string>((id) => this.SearchOnline(id));
        }

        private async Task SaveLyricsInAudioFileAsync()
        {
            this.IsEditing = false;
            this.ParseLyrics(this.lyrics);

            // Save to the file
            var fmd = await this.metadataService.GetFileMetadataAsync(this.track.Path);
            fmd.Lyrics = new MetadataValue() { Value = this.lyrics.Text };

            var fmdList = new List<FileMetadata>
            {
                fmd
            };

            await this.metadataService.UpdateTracksAsync(fmdList, false);
        }

        public LyricsViewModel(IUnityContainer container) : base(container)
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
                                if (FormatUtils.ParseLyricsTime(subString, out time))
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
                RaisePropertyChanged(nameof(this.LyricsLines));
            });
        }

        protected override void SearchOnline(string id)
        {
            this.providerService.SearchOnline(id, new string[] { this.track.ArtistName, this.track.TrackTitle });
        }
    }
}