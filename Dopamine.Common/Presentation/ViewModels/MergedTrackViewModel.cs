using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Scrobbling;
using Dopamine.Common.Database;
using Prism.Mvvm;
using System;
using Digimezzo.Utilities.Settings;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class MergedTrackViewModel : BindableBase
    {
        #region Variables
        private IMetadataService metadataService;
        private IScrobblingService scrobblingService;
        private MergedTrack track;
        private bool isPlaying;
        private bool isPaused;
        private bool showTrackNumber;
        private string artworkPath;
        #endregion

        #region Sorting
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

        #endregion

        #region Properties
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

        public MergedTrack Track
        {
            get { return this.track; }
            set { SetProperty<MergedTrack>(ref this.track, value); }
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
                OnPropertyChanged(() => this.TrackTitle);
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
                    return string.Empty;
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
                OnPropertyChanged(() => this.TrackNumber);
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
                OnPropertyChanged(() => this.AlbumTitle);
            }
        }

        public string AlbumArtist
        {
            get { return this.Track.AlbumArtist; }
            set
            {
                this.Track.AlbumArtist = value;
                OnPropertyChanged(() => this.AlbumArtist);
            }
        }

        public string ArtistName
        {
            get { return this.Track.ArtistName; }
            set
            {
                this.Track.ArtistName = value;
                OnPropertyChanged(() => this.ArtistName);
            }
        }

        
        public string Genre
        {
            get { return this.Track.GenreName; }
            set
            {
                this.Track.GenreName = value;
                OnPropertyChanged(() => this.Genre);
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
                OnPropertyChanged(() => Year);
            }
        }

        public int Rating
        {
            get { return this.Track.Rating.HasValue ? Convert.ToInt32(this.Track.Rating.Value) : 0; }
            set
            {
                this.Track.Rating = (long?)value;
                OnPropertyChanged(() => this.Rating);

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
            OnPropertyChanged(() => this.Love);

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

        public string ArtworkPath
        {
            get { return this.artworkPath; }
            set { SetProperty<string>(ref this.artworkPath, value); }
        }

        public string FileName
        {
            get { return this.Track.FileName; }
        }

        #endregion

        #region Construction
        public MergedTrackViewModel(IMetadataService metadataService, IScrobblingService scrobblingService)
        {
            this.metadataService = metadataService;
            this.scrobblingService = scrobblingService;
        }
        #endregion

        #region Overrides
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

            return this.Track.Equals(((MergedTrackViewModel)obj).Track);
        }

        public override int GetHashCode()
        {
            return this.Track.GetHashCode();
        }
        #endregion

        #region Public
        public void UpdateVisibleRating(int rating)
        {
            this.Track.Rating = (long?)rating;
            OnPropertyChanged(() => this.Rating);
        }

        public void UpdateVisibleLove(bool love)
        {
            this.Track.Love = love ? 1 : 0;
            OnPropertyChanged(() => this.Love);
        }
        #endregion
    }
}
