using Dopamine.Common.Services.Metadata;
using Dopamine.Core.Database;
using System;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class TrackInfoViewModel : BindableBase
    {
        #region Variables
        private IMetadataService metadataService;
        private TrackInfo trackInfo;
        private bool isPlaying;
        private bool isPaused;
        private bool showTrackNumber;
        private string artworkPath;
        #endregion

        #region Properties
        public bool AllowSaveRating { get; set; }

        public TrackInfo TrackInfo
        {
            get { return this.trackInfo; }
            set { SetProperty<TrackInfo>(ref this.trackInfo, value); }
        }

        public string TrackTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(this.TrackInfo.Track.TrackTitle))
                {
                    return this.TrackInfo.Track.TrackTitle;
                }
                else
                {
                    return this.TrackInfo.Track.FileName;
                }

            }
            set
            {
                this.TrackInfo.Track.TrackTitle = value;
                OnPropertyChanged(() => this.TrackTitle);
            }
        }

        public string FormattedTrackNumber
        {
            get
            {
                if (this.TrackInfo.Track.TrackNumber.HasValue && this.TrackInfo.Track.TrackNumber.Value > 0)
                {
                    return string.Format(Convert.ToInt32(TrackInfo.Track.TrackNumber.Value).ToString(), "0");
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
                if (this.TrackInfo.Track.TrackNumber.HasValue && this.TrackInfo.Track.TrackNumber.Value > 0)
                {
                    return this.TrackInfo.Track.TrackNumber.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.TrackInfo.Track.TrackNumber = Convert.ToInt64(value);
                OnPropertyChanged(() => this.TrackNumber);
            }
        }

        public string Duration
        {
            get
            {
                if (this.TrackInfo.Track.Duration.HasValue)
                {
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(this.TrackInfo.Track.Duration));

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

        // SortDuration is used in the Songs DataGrid to correctly sort by Length, 
        // otherwise sorting goes like this: 1:00, 10:00, 2:00, 20:00
        public long SortDuration
        {
            get
            {
                if (this.TrackInfo.Track.Duration.HasValue)
                {
                    return this.TrackInfo.Track.Duration.Value;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string AlbumTitle
        {
            get { return this.TrackInfo.Album.AlbumTitle; }
            set
            {
                this.TrackInfo.Album.AlbumTitle = value;
                OnPropertyChanged(() => this.AlbumTitle);
            }
        }

        public string AlbumArtist
        {
            get { return this.TrackInfo.Album.AlbumArtist; }
            set
            {
                this.TrackInfo.Album.AlbumArtist = value;
                OnPropertyChanged(() => this.AlbumArtist);
            }
        }

        public string ArtistName
        {
            get { return this.TrackInfo.Artist.ArtistName; }
            set
            {
                this.TrackInfo.Artist.ArtistName = value;
                OnPropertyChanged(() => this.ArtistName);
            }
        }

        public string Genre
        {
            get { return this.TrackInfo.Genre.GenreName; }
            set
            {
                this.TrackInfo.Genre.GenreName = value;
                OnPropertyChanged(() => this.Genre);
            }
        }

        public string Year
        {
            get
            {
                if (this.TrackInfo.Track.Year.HasValue && this.TrackInfo.Track.Year.Value > 0)
                {
                    return this.TrackInfo.Track.Year.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.TrackInfo.Track.Year = Convert.ToInt64(value);
                OnPropertyChanged(() => Year);
            }
        }

        public int Rating
        {
            get { return this.TrackInfo.Track.Rating.HasValue ? Convert.ToInt32(this.TrackInfo.Track.Rating.Value) : 0; }
            set
            {
                this.TrackInfo.Track.Rating = (long?)value;
                OnPropertyChanged(() => Rating);

                if (this.AllowSaveRating)
                {
                    this.metadataService.UpdateTrackRatingAsync(this.TrackInfo.Track.Path, value);
                }
            }
        }

        public string GroupHeader
        {
            get
            {
                if (this.TrackInfo.Track.DiscCount.HasValue && this.TrackInfo.Track.DiscCount.Value > 1 && this.TrackInfo.Track.DiscNumber.HasValue && this.TrackInfo.Track.DiscNumber.Value > 0)
                {
                    return string.Format("{0} ({1})", this.TrackInfo.Album.AlbumTitle, this.TrackInfo.Track.DiscNumber);
                }
                else
                {
                    return this.TrackInfo.Album.AlbumTitle;
                }
            }
        }

        public string GroupSubHeader
        {
            get { return this.TrackInfo.Album.AlbumArtist; }
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
            get { return this.TrackInfo.Track.FileName; }
        }

        #endregion

        #region Construction
        public TrackInfoViewModel(IMetadataService metadataService)
        {
            this.metadataService = metadataService;
            this.AllowSaveRating = true;
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

            return this.TrackInfo.Track.Equals(((TrackInfoViewModel)obj).TrackInfo.Track);
        }

        public override int GetHashCode()
        {
            return this.TrackInfo.GetHashCode();
        }
        #endregion
    }
}
