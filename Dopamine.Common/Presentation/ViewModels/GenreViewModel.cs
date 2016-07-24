using Dopamine.Common.Presentation.Interfaces;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Core.Database.Entities;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class GenreViewModel : BindableBase, ISemanticZoomable
    {
        #region Variables
        private Genre genre;
        private bool isHeader;
        #endregion

        #region Properties
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
                OnPropertyChanged(() => this.GenreName);
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
        #endregion

        #region Public
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
        #endregion
    }

}
