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
        public string Bitrate
        {
            get
            {
                return this.TrackInfo.BitRate != null ? this.TrackInfo.BitRate + " kbps" : "";
            }
        }


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
                if (!string.IsNullOrEmpty(this.TrackInfo.TrackTitle))
                {
                    return this.TrackInfo.TrackTitle;
                }
                else
                {
                    return this.TrackInfo.FileName;
                }

            }
            set
            {
                this.TrackInfo.TrackTitle = value;
                OnPropertyChanged(() => this.TrackTitle);
            }
        }

        public string FormattedTrackNumber
        {
            get
            {
                if (this.TrackInfo.TrackNumber.HasValue && this.TrackInfo.TrackNumber.Value > 0)
                {
                    return string.Format(Convert.ToInt32(TrackInfo.TrackNumber.Value).ToString(), "0");
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
                if (this.TrackInfo.TrackNumber.HasValue && this.TrackInfo.TrackNumber.Value > 0)
                {
                    return this.TrackInfo.TrackNumber.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.TrackInfo.TrackNumber = Convert.ToInt64(value);
                OnPropertyChanged(() => this.TrackNumber);
            }
        }

        public string Duration
        {
            get
            {
                if (this.TrackInfo.Duration.HasValue)
                {
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(this.TrackInfo.Duration));

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
                if (this.TrackInfo.Duration.HasValue)
                {
                    return this.TrackInfo.Duration.Value;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string AlbumTitle
        {
            get { return this.TrackInfo.AlbumTitle; }
            set
            {
                this.TrackInfo.AlbumTitle = value;
                OnPropertyChanged(() => this.AlbumTitle);
            }
        }

        public string AlbumArtist
        {
            get { return this.TrackInfo.AlbumArtist; }
            set
            {
                this.TrackInfo.AlbumArtist = value;
                OnPropertyChanged(() => this.AlbumArtist);
            }
        }

        public string ArtistName
        {
            get { return this.TrackInfo.ArtistName; }
            set
            {
                this.TrackInfo.ArtistName = value;
                OnPropertyChanged(() => this.ArtistName);
            }
        }

        public string Genre
        {
            get { return this.TrackInfo.GenreName; }
            set
            {
                this.TrackInfo.GenreName = value;
                OnPropertyChanged(() => this.Genre);
            }
        }

        public string Year
        {
            get
            {
                if (this.TrackInfo.Year.HasValue && this.TrackInfo.Year.Value > 0)
                {
                    return this.TrackInfo.Year.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.TrackInfo.Year = Convert.ToInt64(value);
                OnPropertyChanged(() => Year);
            }
        }

        public int Rating
        {
            get { return this.TrackInfo.Rating.HasValue ? Convert.ToInt32(this.TrackInfo.Rating.Value) : 0; }
            set
            {
                this.TrackInfo.Rating = (long?)value;
                OnPropertyChanged(() => Rating);

                if (this.AllowSaveRating)
                {
                    this.metadataService.UpdateTrackRatingAsync(this.TrackInfo.Path, value);
                }
            }
        }

        public bool Love
        {
            get { return this.TrackInfo.Love.HasValue & this.TrackInfo.Love.Value != 0 ? true : false; }
            set
            {
                this.TrackInfo.Love = value ? 1: 0;
                OnPropertyChanged(() => Love);

                // TODO: update Love on Last.fm + in database
            }
        }

        public string GroupHeader
        {
            get
            {
                if (this.TrackInfo.DiscCount.HasValue && this.TrackInfo.DiscCount.Value > 1 && this.TrackInfo.DiscNumber.HasValue && this.TrackInfo.DiscNumber.Value > 0)
                {
                    return string.Format("{0} ({1})", this.TrackInfo.AlbumTitle, this.TrackInfo.DiscNumber);
                }
                else
                {
                    return this.TrackInfo.AlbumTitle;
                }
            }
        }

        public string GroupSubHeader
        {
            get { return this.TrackInfo.AlbumArtist; }
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
            get { return this.TrackInfo.FileName; }
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

            return this.TrackInfo.Equals(((TrackInfoViewModel)obj).TrackInfo);
        }

        public override int GetHashCode()
        {
            return this.TrackInfo.GetHashCode();
        }
        #endregion
    }
}
