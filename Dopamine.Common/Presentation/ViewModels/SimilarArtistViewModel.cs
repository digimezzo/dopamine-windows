using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class SimilarArtistViewModel : BindableBase
    {
        #region Private
        private string name;
        private string url;
        #endregion

        #region Properties
        public string Name
        {
            get { return this.name; }
            set { SetProperty<string>(ref this.name, value); }
        }

        public string Url
        {
            get { return this.url; }
            set { SetProperty<string>(ref this.url, value); }
        }
        #endregion
    }
}
