using Dopamine.Common.Services.Metadata;
using Dopamine.Common.Services.Scrobbling;
using Dopamine.Core.Database;
using Prism.Mvvm;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class MergedTrackViewModel : BindableBase
    {
        #region Variables
        private IMetadataService metadataService;
        private IScrobblingService scrobblingService;
        private MergedTrack mergedTrack;
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
                return this.MergedTrack.BitRate != null ? this.MergedTrack.BitRate + " kbps" : "";
            }
        }

        public MergedTrack MergedTrack
        {
            get { return this.mergedTrack; }
            set { SetProperty<MergedTrack>(ref this.mergedTrack, value); }
        }

        public string TrackTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(this.MergedTrack.TrackTitle))
                {
                    return this.MergedTrack.TrackTitle;
                }
                else
                {
                    return this.MergedTrack.FileName;
                }

            }
            set
            {
                this.MergedTrack.TrackTitle = value;
                OnPropertyChanged(() => this.TrackTitle);
            }
        }

        public string FormattedTrackNumber
        {
            get
            {
                if (this.MergedTrack.TrackNumber.HasValue && this.MergedTrack.TrackNumber.Value > 0)
                {
                    return string.Format(Convert.ToInt32(this.MergedTrack.TrackNumber.Value).ToString(), "0");
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
                if (this.MergedTrack.TrackNumber.HasValue && this.MergedTrack.TrackNumber.Value > 0)
                {
                    return this.MergedTrack.TrackNumber.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.MergedTrack.TrackNumber = Convert.ToInt64(value);
                OnPropertyChanged(() => this.TrackNumber);
            }
        }

        public string Duration
        {
            get
            {
                if (this.MergedTrack.Duration.HasValue)
                {
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(this.MergedTrack.Duration));

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

        // SortDuration is used in the Songs DataGrid to correctly sort by Length, otherwise sorting goes like this: 1:00, 10:00, 2:00, 20:00
        public long SortDuration
        {
            get
            {
                if (this.MergedTrack.Duration.HasValue)
                {
                    return this.MergedTrack.Duration.Value;
                }
                else
                {
                    return 0;
                }
            }
        }

        // SortAlbumTitle is used to sort by Album, but preserving track nubmer order inside the album
        public string SortAlbumTitle
        {
            get
            {
                return this.AlbumTitle + this.MergedTrack.TrackNumber.Value.ToString("0000");
            }
        }

        public string AlbumTitle
        {
            get { return this.MergedTrack.AlbumTitle; }
            set
            {
                this.MergedTrack.AlbumTitle = value;
                OnPropertyChanged(() => this.AlbumTitle);
            }
        }

        public string AlbumArtist
        {
            get { return this.MergedTrack.AlbumArtist; }
            set
            {
                this.MergedTrack.AlbumArtist = value;
                OnPropertyChanged(() => this.AlbumArtist);
            }
        }

        public string ArtistName
        {
            get { return this.MergedTrack.ArtistName; }
            set
            {
                this.MergedTrack.ArtistName = value;
                OnPropertyChanged(() => this.ArtistName);
            }
        }

        public string Genre
        {
            get { return this.MergedTrack.GenreName; }
            set
            {
                this.MergedTrack.GenreName = value;
                OnPropertyChanged(() => this.Genre);
            }
        }

        public string Year
        {
            get
            {
                if (this.MergedTrack.Year.HasValue && this.MergedTrack.Year.Value > 0)
                {
                    return this.MergedTrack.Year.Value.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                this.MergedTrack.Year = Convert.ToInt64(value);
                OnPropertyChanged(() => Year);
            }
        }

        public int Rating
        {
            get { return this.MergedTrack.Rating.HasValue ? Convert.ToInt32(this.MergedTrack.Rating.Value) : 0; }
            set
            {
                this.MergedTrack.Rating = (long?)value;
                OnPropertyChanged(() => this.Rating);

                this.metadataService.UpdateTrackRatingAsync(this.MergedTrack.Path, value);
            }
        }

        public bool Love
        {
            get { return this.MergedTrack.Love.HasValue && this.MergedTrack.Love.Value != 0 ? true : false; }
            set
            {
                this.SetLoveAsync(value);
            }
        }

        private async void SetLoveAsync(bool love)
        {
            // Update the UI
            this.MergedTrack.Love = love ? 1 : 0;
            OnPropertyChanged(() => this.Love);

            // Update Love in the database
            await this.metadataService.UpdateTrackLoveAsync(this.MergedTrack.Path, love);

            // Send Love/Unlove to the scrobbling service
            await this.scrobblingService.SendTrackLoveAsync(this.MergedTrack, love);
        }

        public string GroupHeader
        {
            get
            {
                if (this.MergedTrack.DiscCount.HasValue && this.MergedTrack.DiscCount.Value > 1 && this.MergedTrack.DiscNumber.HasValue && this.MergedTrack.DiscNumber.Value > 0)
                {
                    return string.Format("{0} ({1})", this.MergedTrack.AlbumTitle, this.MergedTrack.DiscNumber);
                }
                else
                {
                    return this.MergedTrack.AlbumTitle;
                }
            }
        }

        public string GroupSubHeader
        {
            get { return this.MergedTrack.AlbumArtist; }
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
            get { return this.MergedTrack.FileName; }
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

            return this.MergedTrack.Equals(((MergedTrackViewModel)obj).MergedTrack);
        }

        public override int GetHashCode()
        {
            return this.MergedTrack.GetHashCode();
        }
        #endregion

        #region Public
        public void UpdateVisibleRating(int rating)
        {
            this.MergedTrack.Rating = (long?)rating;
            OnPropertyChanged(() => this.Rating);
        }

        public void UpdateVisibleLove(bool love)
        {
            this.MergedTrack.Love = love ? 1 : 0;
            OnPropertyChanged(() => this.Love);
        }
        #endregion
    }
}
