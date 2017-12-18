using Dopamine.Data;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Services.Contracts.Metadata;
using Dopamine.Services.Contracts.Scrobbling;
using Prism.Mvvm;
using System;

namespace Dopamine.Presentation.ViewModels
{
    public class TrackViewModel : BindableBase
    {
        private IMetadataService metadataService;
        private IScrobblingService scrobblingService;
        private PlayableTrack track;
        private bool isPlaying;
        private bool isPaused;
        private bool showTrackNumber;
        private bool showTrackArt;
        private byte[] trackArt;
 
        // SortDuration is used to correctly sort by Length, otherwise sorting goes like this: 1:00, 10:00, 2:00, 20:00.
        public long SortDuration
        {
            get
            {
                if (this.Track.Duration.HasValue)
                {
                    return this.Track.Duration.Value;
                }
                else
                {
                    return 0;
                }
            }
        }

        // SortAlbumTitle is used to sort by AlbumTitle, then by TrackNumber.
        public string SortAlbumTitle
        {
            get
            {
                return this.AlbumTitle + this.track.TrackNumber.Value.ToString("0000");
            }
        }

        // SortAlbumArtist is used to sort by AlbumArtist, then by AlbumTitle, then by TrackNumber.
        public string SortAlbumArtist
        {
            get
            {
                return this.AlbumArtist + this.AlbumTitle + this.track.TrackNumber.Value.ToString("0000");
            }
        }

        // SortArtistName is used to sort by AlbumArtist, then by AlbumTitle, then by TrackNumber.
        public string SortArtistName
        {
            get
            {
                return this.ArtistName + this.AlbumTitle + this.track.TrackNumber.Value.ToString("0000");
            }
        }

        public int SortBitrate
        {
            get
            {
                return Convert.ToInt32(Track.BitRate ?? 0);
            }
        }

        public bool ShowTrackArt
        {
            get { return this.showTrackArt; }
            set
            {
                bool oldValue = this.showTrackArt;
                SetProperty(ref this.showTrackArt, value);

                if (oldValue != value && value)
                {
                    this.GetTrackArt();
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

        public async void GetTrackArt()
        {
            try
            {
                this.TrackArt = await this.metadataService.GetArtworkAsync(this.Track.Path);
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        public bool HasLyrics
        {
            get
            {
                return this.Track.HasLyrics == 1 ? true : false;
            }
        }

        public string Bitrate
        {
            get
            {
                return this.Track.BitRate != null ? this.Track.BitRate + " kbps" : "";
            }
        }

        public PlayableTrack Track
        {
            get { return this.track; }
            set { SetProperty<PlayableTrack>(ref this.track, value); }
        }

        public string TrackTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Track.TrackTitle))
                {
                    return this.Track.TrackTitle;
                }
                else
                {
                    return this.Track.FileName;
                }

            }
            set
            {
                this.Track.TrackTitle = value;
                RaisePropertyChanged(nameof(this.TrackTitle));
            }
        }

        public string FormattedTrackNumber
        {
            get
            {
                if (this.Track.TrackNumber.HasValue && this.Track.TrackNumber.Value > 0)
                {
                    return string.Format(Convert.ToInt32(Track.TrackNumber.Value).ToString(), "0");
                }
                else
                {
                    return "--";
                }
            }
        }

        public string TrackNumber
        {
            get
            {
                if (this.Track.TrackNumber.HasValue && this.Track.TrackNumber.Value > 0)
                {
                    return this.Track.TrackNumber.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.Track.TrackNumber = Convert.ToInt64(value);
                RaisePropertyChanged(nameof(this.TrackNumber));
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

        public string AlbumTitle
        {
            get { return this.Track.AlbumTitle; }
            set
            {
                this.Track.AlbumTitle = value;
                RaisePropertyChanged(nameof(this.AlbumTitle));
            }
        }

        public string PlayCount
        {
            get { return this.Track.PlayCount > 0 ? this.Track.PlayCount.ToString() : string.Empty; }
        }

        public string SkipCount
        {
            get { return this.Track.SkipCount > 0 ? this.Track.SkipCount.ToString() : string.Empty; }
        }

        public string DateLastPlayed
        {
            get { return this.Track.DateLastPlayed > 0 ? new DateTime(this.Track.DateLastPlayed.Value).ToString("g") : string.Empty; }
        }

        public long SortDateLastPlayed
        {
            get { return this.Track.DateLastPlayed.Value; }
        }

        public string AlbumArtist
        {
            get { return this.Track.AlbumArtist; }
            set
            {
                this.Track.AlbumArtist = value;
                RaisePropertyChanged(nameof(this.AlbumArtist));
            }
        }

        public string ArtistName
        {
            get { return this.Track.ArtistName; }
            set
            {
                this.Track.ArtistName = value;
                RaisePropertyChanged(nameof(this.ArtistName));
            }
        }


        public string Genre
        {
            get { return this.Track.GenreName; }
            set
            {
                this.Track.GenreName = value;
                RaisePropertyChanged(nameof(this.Genre));
            }
        }

        public string Year
        {
            get
            {
                if (this.Track.Year.HasValue && this.Track.Year.Value > 0)
                {
                    return this.Track.Year.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.Track.Year = Convert.ToInt64(value);
                RaisePropertyChanged(nameof(this.Year));
            }
        }

        public int Rating
        {
            get { return this.Track.Rating.HasValue ? Convert.ToInt32(this.Track.Rating.Value) : 0; }
            set
            {
                this.Track.Rating = (long?)value;
                RaisePropertyChanged(nameof(this.Rating));

                this.metadataService.UpdateTrackRatingAsync(this.Track.Path, value);
            }
        }

        public bool Love
        {
            get { return this.Track.Love.HasValue && this.Track.Love.Value != 0 ? true : false; }
            set
            {
                this.SetLoveAsync(value);
            }
        }

        private async void SetLoveAsync(bool love)
        {
            // Update the UI
            this.Track.Love = love ? 1 : 0;
            RaisePropertyChanged(nameof(this.Love));

            // Update Love in the database
            await this.metadataService.UpdateTrackLoveAsync(this.Track.Path, love);

            // Send Love/Unlove to the scrobbling service
            await this.scrobblingService.SendTrackLoveAsync(this.Track, love);
        }

        public string GroupHeader
        {
            get
            {
                if (this.Track.DiscCount.HasValue && this.Track.DiscCount.Value > 1 && this.Track.DiscNumber.HasValue && this.Track.DiscNumber.Value > 0)
                {
                    return string.Format("{0} ({1})", this.Track.AlbumTitle, this.Track.DiscNumber);
                }
                else
                {
                    return this.Track.AlbumTitle;
                }
            }
        }

        public string GroupSubHeader
        {
            get { return this.Track.AlbumArtist; }
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

        public string FileName
        {
            get { return this.Track.FileName; }
        }

        public TrackViewModel(IMetadataService metadataService, IScrobblingService scrobblingService)
        {
            this.metadataService = metadataService;
            this.scrobblingService = scrobblingService;
        }
    
        public override string ToString()
        {
            return this.TrackTitle;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Track.Equals(((TrackViewModel)obj).Track);
        }

        public override int GetHashCode()
        {
            return this.Track.GetHashCode();
        }
     
        public void UpdateVisibleRating(int rating)
        {
            this.Track.Rating = (long?)rating;
            RaisePropertyChanged(nameof(this.Rating));
        }

        public void UpdateVisibleLove(bool love)
        {
            this.Track.Love = love ? 1 : 0;
            RaisePropertyChanged(nameof(this.Love));
        }

        public void UpdateVisibleCounters(TrackStatistic statistic)
        {
            this.Track.PlayCount = statistic.PlayCount;
            this.Track.SkipCount = statistic.SkipCount;
            this.track.DateLastPlayed = statistic.DateLastPlayed;
            RaisePropertyChanged(nameof(this.PlayCount));
            RaisePropertyChanged(nameof(this.SkipCount));
            RaisePropertyChanged(nameof(this.DateLastPlayed));
            RaisePropertyChanged(nameof(this.SortDateLastPlayed));
        }
    }
}
