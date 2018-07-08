using Digimezzo.Utilities.Utils;
using Dopamine.Data;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class AlbumViewModel : BindableBase
    {
        private string albumTitle;
        private string albumArtist;
        private string year;
        private string artworkPath;
        private string mainHeader;
        private string subHeader;
        private long? dateAdded;
        private long? dateFileCreated;
        private long sortYear;

        public AlbumViewModel(AlbumData albumData, bool setUnknownStrings)
        {
            this.albumTitle = !string.IsNullOrEmpty(albumData.AlbumTitle) || !setUnknownStrings ? albumData.AlbumTitle : ResourceUtils.GetString("Language_Unknown_Album");
            this.albumArtist = !string.IsNullOrEmpty(albumData.AlbumArtists) || !setUnknownStrings ? MetadataUtils.GetCommaSeparatedMultiValueTags(albumData.AlbumArtists) : ResourceUtils.GetString("Language_Unknown_Artist");
            this.year = albumData.Year.HasValue && albumData.Year.Value > 0 ? albumData.Year.Value.ToString() : string.Empty;
            this.SortYear = albumData.Year.HasValue ? albumData.Year.Value : 0;
            this.AlbumKey = albumData.AlbumKey;
            this.DateAdded = albumData.DateAdded;
            this.DateFileCreated = albumData.DateFileCreated;
        }

        public string AlbumKey { get; set; }

        public long? DateAdded
        {
            get { return this.dateAdded; }
            set
            {
                SetProperty<long?>(ref this.dateAdded, value);
            }
        }

        public long? DateFileCreated
        {
            get { return this.dateFileCreated; }
            set
            {
                SetProperty<long?>(ref this.dateFileCreated, value);
            }
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
            set
            {
                SetProperty<string>(ref this.year, value);
                RaisePropertyChanged(nameof(this.ToolTipYear));
            }
        }

        public long SortYear
        {
            get { return this.sortYear; }
            set
            {
                SetProperty<long>(ref this.sortYear, value);
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
            return this.albumTitle;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.AlbumKey.Equals(((AlbumViewModel)obj).AlbumKey);
        }

        public override int GetHashCode()
        {
            return this.AlbumKey.GetHashCode();
        }
    }
}
