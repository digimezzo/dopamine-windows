using Dopamine.Core.Base;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class AlbumViewModel : BindableBase
    {
        private string albumTitle;
        private string albumArtist;
        private string year;
        private string albumKey;
        private string artworkPath;
        private string mainHeader;
        private string subHeader;

        public AlbumViewModel(string albumTitle, string albumArtists, long? year, string albumKey)
        {
            this.albumTitle = albumTitle;
            this.albumArtist = albumArtists.Replace(Constants.MultiValueTagsSeparator, ", ");
            this.year = year == null || year == 0 ? string.Empty : year.ToString();
            this.albumKey = albumKey;
        }

        public double Opacity { get; set; }

        public bool HasCover
        {
            get { return !string.IsNullOrEmpty(this.artworkPath); }
        }

        public bool HasTitle
        {
            get { return !string.IsNullOrEmpty(this.AlbumTitle); }
        }

        public string ToolTipYear
        {
            get { return !string.IsNullOrEmpty(this.year) ? "(" + this.year + ")" : string.Empty; }
        }

        public string Year
        {
            get { return this.year; }
            set {
                SetProperty<string>(ref this.year, value);
                RaisePropertyChanged(nameof(this.ToolTipYear));
            }
        }

        public string AlbumTitle
        {
            get { return this.albumTitle; }
            set
            {
                SetProperty<string>(ref this.albumTitle, value);
                RaisePropertyChanged(nameof(this.HasTitle));
            }
        }

        public string AlbumArtist
        {
            get { return this.albumArtist; }
            set { SetProperty<string>(ref this.albumArtist, value); }
        }

        public string ArtworkPath
        {
            get { return this.artworkPath; }
            set {
                SetProperty<string>(ref this.artworkPath, value);
                RaisePropertyChanged(nameof(this.HasCover));
            }
        }

        public string MainHeader
        {
            get { return this.mainHeader; }
            set { SetProperty<string>(ref this.mainHeader, value); }
        }

        public string SubHeader
        {
            get { return this.subHeader; }
            set { SetProperty<string>(ref this.subHeader, value); }
        }

        public override string ToString()
        {
            return this.albumTitle;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.albumKey.Equals(((AlbumViewModel)obj).albumKey);
        }

        public override int GetHashCode()
        {
            return this.albumKey.GetHashCode();
        }
    }
}
