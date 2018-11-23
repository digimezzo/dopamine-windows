using Digimezzo.Utilities.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using Dopamine.Core.Utils;
using Dopamine.Data;
using Dopamine.Data.Entities;
using Dopamine.Services.Metadata;
using Dopamine.Services.Scrobbling;
using Prism.Mvvm;
using System;

namespace Dopamine.Services.Entities
{
    public class TrackViewModel : BindableBase
    {
        private int scaledTrackCoverSize = Convert.ToInt32(Constants.TrackCoverSize * Constants.CoverUpscaleFactor);
        private IMetadataService metadataService;
        private IScrobblingService scrobblingService;
        private bool isPlaying;
        private bool isPaused;
        private bool showTrackNumber;
        private bool showTrackArt;
        private byte[] trackArt;

        public TrackViewModel(IMetadataService metadataService, IScrobblingService scrobblingService, Track track)
        {
            this.metadataService = metadataService;
            this.scrobblingService = scrobblingService;
            this.Track = track;
        }

        public string PlaylistEntry { get; set; }

        public bool IsPlaylistEntry => !string.IsNullOrEmpty(this.PlaylistEntry);

        public Track Track { get; private set; }

        // SortDuration is used to correctly sort by Length, otherwise sorting goes like this: 1:00, 10:00, 2:00, 20:00.
        public long SortDuration => this.Track.Duration.HasValue ? this.Track.Duration.Value : 0;

        // SortAlbumTitle is used to sort by AlbumTitle, then by TrackNumber.
        public string SortAlbumTitle => this.Track.AlbumTitle + this.Track.TrackNumber.Value.ToString("0000");

        // SortAlbumArtist is used to sort by AlbumArtists, then by AlbumTitle, then by TrackNumber.
        public string SortAlbumArtist => this.Track.AlbumArtists + this.AlbumTitle + this.Track.TrackNumber.Value.ToString("0000");

        // SortArtistName is used to sort by ArtistName, then by AlbumTitle, then by TrackNumber.
        public string SortArtistName => this.ArtistName + this.AlbumTitle + this.Track.TrackNumber.Value.ToString("0000");

        public long SortBitrate => this.Track.BitRate.GetValueOrZero();

        public string SortPlayCount => this.Track.PlayCount.HasValueLargerThan(0) ? this.Track.PlayCount.Value.ToString("0000") : string.Empty;

        public string SortSkipCount => this.Track.SkipCount.HasValueLargerThan(0) ? this.Track.SkipCount.Value.ToString("0000") : string.Empty;

        public long SortTrackNumber => this.Track.TrackNumber.HasValue ? this.Track.TrackNumber.Value : 0;

        public bool HasLyrics => this.Track.HasLyrics == 1 ? true : false;

        public string Bitrate => this.Track.BitRate != null ? this.Track.BitRate + " kbps" : "";

        public string AlbumTitle => string.IsNullOrEmpty(this.Track.AlbumTitle) ? ResourceUtils.GetString("Language_Unknown_Album") : this.Track.AlbumTitle;

        public string PlayCount => this.Track.PlayCount.HasValueLargerThan(0) ? this.Track.PlayCount.Value.ToString() : string.Empty;

        public string SkipCount => this.Track.SkipCount.HasValueLargerThan(0) ? this.Track.SkipCount.Value.ToString() : string.Empty;

        public string DateLastPlayed => this.Track.DateLastPlayed.HasValueLargerThan(0) ? new DateTime(this.Track.DateLastPlayed.Value).ToString("g") : string.Empty;

        public long SortDateLastPlayed => this.Track.DateLastPlayed.Value;

        public string TrackTitle => string.IsNullOrEmpty(this.Track.TrackTitle) ? this.Track.FileName : this.Track.TrackTitle;

        public string FileName => this.Track.FileName;

        public string Path => this.Track.Path;

        public string SafePath => this.Track.SafePath;

        public string ArtistName => string.IsNullOrEmpty(this.Track.Artists) ? ResourceUtils.GetString("Language_Unknown_Artist") : DataUtils.GetCommaSeparatedColumnMultiValue(this.Track.Artists);

        public string AlbumArtist => string.IsNullOrEmpty(this.Track.AlbumArtists) ? ResourceUtils.GetString("Language_Unknown_Artist") : DataUtils.GetCommaSeparatedColumnMultiValue(this.Track.AlbumArtists);

        public string Genre => string.IsNullOrEmpty(this.Track.Genres) ? ResourceUtils.GetString("Language_Unknown_Genres") : DataUtils.GetCommaSeparatedColumnMultiValue(this.Track.Genres);

        public string FormattedTrackNumber => this.Track.TrackNumber.HasValueLargerThan(0) ? Track.TrackNumber.Value.ToString("00") : "--";

        public string TrackNumber => this.Track.TrackNumber.HasValueLargerThan(0) ? this.Track.TrackNumber.ToString() : string.Empty;

        public string DiscNumber => this.Track.DiscNumber.HasValueLargerThan(0) ? this.Track.DiscNumber.ToString() : string.Empty;

        public string Year => this.Track.Year.HasValueLargerThan(0) ? this.Track.Year.Value.ToString() : string.Empty;

        public string GroupHeader => this.Track.DiscCount.HasValueLargerThan(1) && this.Track.DiscNumber.HasValueLargerThan(0) ? $"{this.Track.AlbumTitle} ({this.Track.DiscNumber})" : this.Track.AlbumTitle;

        public string GroupSubHeader => this.AlbumArtist;

        public bool ShowTrackArt
        {
            get { return this.showTrackArt; }
            set
            {
                SetProperty(ref this.showTrackArt, value);

                if (value)
                {
                    if(this.trackArt == null || this.trackArt.Length == 0)
                    {
                        this.GetTrackArt();
                    }
                }
                else
                {
                    this.TrackArt = null;
                }
            }
        }

        public byte[] TrackArt
        {
            get
            {
                return this.trackArt;
            }
            private set
            {
                SetProperty(ref this.trackArt, value);
            }
        }

        private async void GetTrackArt()
        {
            try
            {
                this.TrackArt = await this.metadataService.GetArtworkAsync(this.Track.Path, scaledTrackCoverSize);
            }
            catch (Exception)
            {
                // Intended suppression
            }
        }

        public string Duration
        {
            get
            {
                if (this.Track.Duration.HasValue)
                {
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(this.Track.Duration));

                    if (ts.Hours > 0)
                    {
                        return ts.ToString("hh\\:mm\\:ss");
                    }
                    else
                    {
                        return ts.ToString("m\\:ss");
                    }
                }
                else
                {
                    return "0:00";
                }
            }
        }

        public int Rating
        {
            get { return NumberUtils.ConvertToInt32(this.Track.Rating); }
            set
            {
                // Update the UI
                this.Track.Rating = (long?)value;
                this.RaisePropertyChanged(nameof(this.Rating));

                // Update Rating in the database
                this.metadataService.UpdateTrackRatingAsync(this.Track.Path, value);
            }
        }

        public bool Love
        {
            get { return this.Track.Love.HasValue && this.Track.Love.Value != 0 ? true : false; }
            set
            {
                // Update the UI
                this.Track.Love = value ? 1 : 0;
                this.RaisePropertyChanged(nameof(this.Love));

                // Update Love in the database
                this.metadataService.UpdateTrackLoveAsync(this.Track.Path, value);

                // Send Love/Unlove to the scrobbling service
                this.scrobblingService.SendTrackLoveAsync(this, value);
            }
        }

        public bool IsPlaying
        {
            get { return this.isPlaying; }
            set { SetProperty<bool>(ref this.isPlaying, value); }
        }

        public bool IsPaused
        {
            get { return this.isPaused; }
            set { SetProperty<bool>(ref this.isPaused, value); }
        }

        public bool ShowTrackNumber
        {
            get { return this.showTrackNumber; }
            set { SetProperty<bool>(ref this.showTrackNumber, value); }
        }

        public void UpdateVisibleRating(int rating)
        {
            this.Track.Rating = (long?)rating;
            this.RaisePropertyChanged(nameof(this.Rating));
        }

        public void UpdateVisibleLove(bool love)
        {
            this.Track.Love = love ? 1 : 0;
            this.RaisePropertyChanged(nameof(this.Love));
        }

        public void UpdateVisibleCounters(PlaybackCounter counters)
        {
            this.Track.PlayCount = counters.PlayCount;
            this.Track.SkipCount = counters.SkipCount;
            this.Track.DateLastPlayed = counters.DateLastPlayed;
            this.RaisePropertyChanged(nameof(this.PlayCount));
            this.RaisePropertyChanged(nameof(this.SkipCount));
            this.RaisePropertyChanged(nameof(this.DateLastPlayed));
            this.RaisePropertyChanged(nameof(this.SortDateLastPlayed));
        }

        public override string ToString()
        {
            return this.TrackTitle;
        }

        public TrackViewModel DeepCopy()
        {
            return new TrackViewModel(this.metadataService, this.scrobblingService, this.Track);
        }

        public void UpdateTrack(Track track)
        {
            if(track == null)
            {
                return;
            }

            this.Track = track;

            this.RaisePropertyChanged();
        }

        public void Refresh()
        {
            this.RaisePropertyChanged();
        }
    }
}
