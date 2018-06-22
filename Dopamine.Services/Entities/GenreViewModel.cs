using Dopamine.Services.Utils;
using Prism.Mvvm;
using System;

namespace Dopamine.Services.Entities
{
    public class GenreViewModel : BindableBase, ISemanticZoomable
    {
        private string genreName;
        private bool isHeader;

        public GenreViewModel(string genreName, bool isHeader)
        {
            this.genreName = genreName;
            this.isHeader = isHeader;
        }
      
        public string GenreName
        {
            get { return this.genreName; }
            set {
                this.genreName = value;
                RaisePropertyChanged(nameof(this.GenreName));
            }
        }

        public string Header
        {
            get { return SemanticZoomUtils.GetGroupHeader(this.genreName); }
        }

        public bool IsHeader
        {
            get { return this.isHeader; }
            set { SetProperty<bool>(ref this.isHeader, value); }
        }
     
        public override string ToString()
        {

            return this.GenreName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return string.Equals(this.genreName, ((GenreViewModel)obj).genreName, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.genreName.GetHashCode();
        }
    }
}
