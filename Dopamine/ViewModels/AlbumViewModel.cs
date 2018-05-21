using Dopamine.Data.Entities;
using Prism.Mvvm;

namespace Dopamine.ViewModels
{
    public class AlbumViewModel : BindableBase
    {
        private Album album;
        private string artworkPath;
        private string mainHeader;
        private string subHeader;

        public double Opacity { get; set; }

        public bool HasCover
        {
            get { return !string.IsNullOrEmpty(this.artworkPath); }
        }

        public bool HasTitle
        {
            get { return !string.IsNullOrEmpty(this.Album.AlbumTitle); }
        }

        public string Year
        {
            get { return this.Album.Year.HasValue && this.Album.Year.Value > 0 ? this.Album.Year.ToString() : string.Empty; }
        }

        public string ToolTipYear
        {
            get { return !string.IsNullOrEmpty(this.Year) ? "(" + this.Year + ")" : string.Empty; }
        }

        public Album Album
        {
            get { return this.album; }
            set { SetProperty<Album>(ref this.album, value); }
        }

        public string AlbumTitle
        {
            get { return this.Album.AlbumTitle; }
            set {
                this.Album.AlbumTitle = value;
                RaisePropertyChanged(nameof(this.AlbumTitle));
            }
        }

        public string AlbumArtist
        {
            get { return this.Album.AlbumArtist; }
            set {
                this.Album.AlbumArtist = value;
                RaisePropertyChanged(nameof(this.AlbumArtist));
            }
        }

        public string ArtworkPath
        {
            get { return this.artworkPath; }
            set
            {
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
            return this.Album.AlbumTitle;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Album.Equals(((AlbumViewModel)obj).Album);
        }

        public override int GetHashCode()
        {
            return this.Album.GetHashCode();
        }
    }
}
