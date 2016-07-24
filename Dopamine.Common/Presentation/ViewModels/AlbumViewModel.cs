using Dopamine.Core.Database.Entities;
using Microsoft.Practices.Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class AlbumViewModel : BindableBase
    {
        #region Variables
        private Album album;
        private string artworkPath;
        private string mainHeader;
        private string subHeader;
        #endregion

        #region ReadOnly Properties
        public string Year
        {
            get { return this.Album.Year.HasValue && this.Album.Year.Value > 0 ? this.Album.Year.ToString() : string.Empty; }
        }

        public string ToolTipYear
        {
            get { return !string.IsNullOrEmpty(this.Year) ? "(" + this.Year + ")" : string.Empty; }
        }
        #endregion

        #region Properties
        public bool HasCover
        {
            get { return !string.IsNullOrEmpty(this.artworkPath); }
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
                OnPropertyChanged(() => this.AlbumTitle);
            }
        }

        public string AlbumArtist
        {
            get { return this.Album.AlbumArtist; }
            set {
                this.Album.AlbumArtist = value;
                OnPropertyChanged(() => this.AlbumArtist);
            }
        }

        public string ArtworkPath
        {
            get { return this.artworkPath; }
            set
            {
                SetProperty<string>(ref this.artworkPath, value);
                OnPropertyChanged(() => this.HasCover);
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
        #endregion

        #region Overrides
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
        #endregion
    }
}
