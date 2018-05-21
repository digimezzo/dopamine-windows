using Dopamine.Core.Utils;
using Dopamine.Data.Contracts.Entities;
using Dopamine.Interfaces;
using Dopamine.Utils;
using Prism.Mvvm;

namespace Dopamine.ViewModels
{
    public class ArtistViewModel : BindableBase, ISemanticZoomable
    {
        private Artist artist;
        private bool isHeader;
        public Artist Artist
        {
            get { return this.artist; }
            set { SetProperty<Artist>(ref this.artist, value); }
        }

        public string ArtistName
        {
            get { return this.Artist.ArtistName; }
            set
            {
                this.Artist.ArtistName = value;
                RaisePropertyChanged(nameof(this.ArtistName));
            }
        }

        public string SortArtistName
        {
            get { return FormatUtils.GetSortableString(this.ArtistName, true); }
        }

        public string Header
        {
            get { return SemanticZoomUtils.GetGroupHeader(this.Artist.ArtistName, true); }
        }

        public bool IsHeader
        {
            get { return this.isHeader; }
            set { SetProperty<bool>(ref this.isHeader, value); }
        }
       
        public override string ToString()
        {
            return this.ArtistName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Artist.Equals(((ArtistViewModel)obj).Artist);
        }

        public override int GetHashCode()
        {
            return this.Artist.GetHashCode();
        }
    }
}
