using Dopamine.Services.Entities;

namespace Dopamine.Services.ExternalControl
{
    /// <summary>
    /// ExternalControlServer can't use TrackViewModel directly, as it is not serializable.
    /// </summary>
    public class ExternalTrack
    {
        private TrackViewModel trackViewModel;

        public ExternalTrack()
        {
        }

        public ExternalTrack(TrackViewModel trackViewModel)
        {
            this.trackViewModel = trackViewModel;

            this.BitRate = this.trackViewModel?.Track?.BitRate != null ? this.trackViewModel.Track.BitRate.Value : 0;
            this.Duration = this.trackViewModel?.Track?.Duration != null ? this.trackViewModel.Track.Duration.Value : 0;
            this.FileName = !string.IsNullOrEmpty(this.trackViewModel.Track.FileName) ? this.trackViewModel.Track.FileName : "";
            this.Path = !string.IsNullOrEmpty(this.trackViewModel.Track.Path) ? this.trackViewModel.Track.Path : "";
            this.SampleRate = this.trackViewModel?.Track?.SampleRate != null ? this.trackViewModel.Track.SampleRate.Value : 0;
            this.TrackNumber = this.trackViewModel?.Track?.TrackNumber != null ? this.trackViewModel.Track.TrackNumber.Value.ToString() : "";
            this.TrackTitle = !string.IsNullOrEmpty(this.trackViewModel.TrackTitle) ? this.trackViewModel.TrackTitle : "";
            this.Year = !string.IsNullOrEmpty(this.trackViewModel.Year) ? this.trackViewModel.Year : "";
            this.AlbumArtist = !string.IsNullOrEmpty(this.trackViewModel.AlbumArtist) ? this.trackViewModel.AlbumArtist : "";
            this.AlbumTitle = !string.IsNullOrEmpty(this.trackViewModel.AlbumTitle) ? this.trackViewModel.AlbumTitle : "";
            this.ArtistName = !string.IsNullOrEmpty(this.trackViewModel.ArtistName) ? this.trackViewModel.ArtistName : "";
            this.Genre = !string.IsNullOrEmpty(this.trackViewModel.Genre) ? this.trackViewModel.Genre : "";
            this.Love = this.trackViewModel?.Track?.Love != null ? this.trackViewModel.Track.Love.Value : 0;
            this.PlayCount = this.trackViewModel?.Track?.PlayCount != null ? this.trackViewModel.Track.PlayCount.Value : 0;
            this.Rating = this.trackViewModel?.Track?.Rating != null ? this.trackViewModel.Track.Rating.Value : 0;
            this.SkipCount = this.trackViewModel?.Track?.SkipCount != null ? this.trackViewModel.Track.SkipCount.Value : 0;
        }

        public long BitRate { get; set; }

        public long Duration { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public long SampleRate { get; set; }

        public string TrackNumber { get; set; }

        public string TrackTitle { get; set; }

        public string Year { get; set; }

        public string AlbumArtist { get; set; }

        public string AlbumTitle { get; set; }

        public string ArtistName { get; set; }

        public string Genre { get; set; }

        public long Love { get; set; }

        public long PlayCount { get; set; }

        public long Rating { get; set; }

        public long SkipCount { get; set; }
    }
}
