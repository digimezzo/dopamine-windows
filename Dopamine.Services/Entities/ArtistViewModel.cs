using Dopamine.Core.Utils;
using Dopamine.Services.Utils;
using Prism.Mvvm;
using System;

namespace Dopamine.Services.Entities
{
    public class ArtistViewModel : BindableBase, ISemanticZoomable
    {
        private string artistName;
        private bool isHeader;

        public ArtistViewModel(string artistName, bool isHeader)
        {
            this.artistName = artistName;
            this.isHeader = isHeader;
        }

        public string ArtistName
        {
            get { return this.artistName; }
            set
            {
                this.ArtistName = value;
                RaisePropertyChanged(nameof(this.ArtistName));
            }
        }

        public string SortArtistName
        {
            get { return FormatUtils.GetSortableString(this.artistName, true); }
        }

        public string Header
        {
            get { return SemanticZoomUtils.GetGroupHeader(this.artistName, true); }
        }

        public bool IsHeader
        {
            get { return this.isHeader; }
            set { SetProperty<bool>(ref this.isHeader, value); }
        }
       
        public override string ToString()
        {
            return this.artistName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return string.Equals(this.artistName, ((ArtistViewModel)obj).artistName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.artistName.GetHashCode();
        }
    }
}
