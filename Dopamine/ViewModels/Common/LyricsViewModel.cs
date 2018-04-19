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
using Dopamine.Presentation.Utils;
using Digimezzo.Utilities.Log;

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

        public LyricsViewModel(IContainerProvider container, PlayableTrack track) : this(container)
        {
            this.track = track;
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

        public void SetLyrics(Lyrics lyrics)
        {
            this.Lyrics = lyrics;
            this.uneditedLyrics = lyrics;
            this.ParseLyrics(lyrics);
        }

        private void ParseLyrics(Lyrics lyrics)
        {
            Application.Current.Dispatcher.Invoke(() => this.lyricsLines = null);

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.lyricsLines = new ObservableCollection<LyricsLineViewModel>(LyricsUtils.ParseLyrics(lyrics));
                RaisePropertyChanged(nameof(this.LyricsLines));
            });
            ;
        }

        protected override void SearchOnline(string id)
        {
            this.providerService.SearchOnline(id, new string[] { this.track.ArtistName, this.track.TrackTitle });
        }
    }
}