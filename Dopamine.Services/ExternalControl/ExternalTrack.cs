using Dopamine.Services.Entities;
using System;
using System.Runtime.Serialization;

namespace Dopamine.Services.ExternalControl
{
    /// <summary>
    /// ExternalControlServer can't use TrackViewModel directly, as it is not serializable.
    /// </summary>
    [DataContract]
    public class ExternalTrack
    {
        private TrackViewModel trackViewModel;

        public ExternalTrack(TrackViewModel trackViewModel)
        {
            this.trackViewModel = trackViewModel;
        }

        [DataMember]
        public long BitRate => this.trackViewModel.Track.BitRate.Value;

        [DataMember]
        public long Duration => this.trackViewModel.Track.Duration.Value;

        [DataMember]
        public string FileName => this.trackViewModel.Track.FileName;

        [DataMember]
        public string Path => this.trackViewModel.Track.Path;

        [DataMember]
        public long SampleRate => this.trackViewModel.Track.SampleRate.Value;

        [DataMember]
        public string TrackNumber => this.trackViewModel.TrackNumber;

        [DataMember]
        public string TrackTitle => this.trackViewModel.TrackTitle;

        [DataMember]
        public string Year => this.trackViewModel.Year;

        [DataMember]
        public string AlbumArtist => this.trackViewModel.AlbumArtist;

        [DataMember]
        public string AlbumTitle => this.trackViewModel.AlbumTitle;

        [DataMember]
        public string ArtistName => this.trackViewModel.ArtistName;

        [DataMember]
        public string Genre => this.trackViewModel.Genre;

        [DataMember]
        public long Love => this.trackViewModel.Track.Love.Value;

        [DataMember]
        public long PlayCount => this.trackViewModel.Track.PlayCount.Value;

        [DataMember]
        public long Rating => this.trackViewModel.Track.Rating.Value;

        [DataMember]
        public long SkipCount => this.trackViewModel.Track.SkipCount.Value;
    }
}
