using Dopamine.Data.Entities;
using Dopamine.Presentation.Interfaces;
using Dopamine.Presentation.Utils;
using Prism.Mvvm;

namespace Dopamine.Presentation.ViewModels
{
    public class GenreViewModel : BindableBase, ISemanticZoomable
    {
        private Genre genre;
        private bool isHeader;
        public Genre Genre
        {
            get { return this.genre; }
            set { SetProperty<Genre>(ref this.genre, value); }
        }

        public string GenreName
        {
            get { return this.Genre.GenreName; }
            set {
                this.Genre.GenreName = value;
                RaisePropertyChanged(nameof(this.GenreName));
            }
        }

        public string Header
        {
            get { return SemanticZoomUtils.GetGroupHeader(this.Genre.GenreName); }
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

            return this.Genre.Equals(((GenreViewModel)obj).Genre);
        }

        public override int GetHashCode()
        {
            return this.Genre.GetHashCode();
        }
    }
}
