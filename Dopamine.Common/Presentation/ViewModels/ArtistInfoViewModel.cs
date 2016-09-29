using Dopamine.Core.API.Lastfm;
using Prism.Mvvm;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ArtistInfoViewModel : BindableBase
    {
        #region Variables
        private LastFmArtist lfmArtist;
        private ObservableCollection<SimilarArtistViewModel> similarArtists;
        #endregion

        #region Properties
        public bool HasBiography
        {
            get
            {
                return this.Biography != null && !string.IsNullOrWhiteSpace(this.Biography.Content);
            }
        }

        public bool HasSimilarArtists
        {
            get { return this.SimilarArtists != null && this.SimilarArtists.Count > 0; }
        }

        public bool HasImage
        {
            get { return !string.IsNullOrEmpty(this.Image); }
        }

        public ObservableCollection<SimilarArtistViewModel> SimilarArtists
        {
            get { return this.similarArtists; }
            set { SetProperty<ObservableCollection<SimilarArtistViewModel>>(ref this.similarArtists, value); }
        }

        public LastFmArtist LfmArtist
        {
            get { return this.lfmArtist; }
            set
            {
                SetProperty<LastFmArtist>(ref this.lfmArtist, value);

                this.FillSimilarArtists();

                OnPropertyChanged(() => this.Biography);
                OnPropertyChanged(() => this.HasBiography);
                OnPropertyChanged(() => this.Url);
                OnPropertyChanged(() => this.CleanedBiographyContent);
                OnPropertyChanged(() => this.UrlText);
                OnPropertyChanged(() => this.ArtistName);
                OnPropertyChanged(() => this.Image);
                OnPropertyChanged(() => this.HasImage);
                OnPropertyChanged(() => this.SimilarArtists);
                OnPropertyChanged(() => this.HasSimilarArtists);
            }
        }

        public string Image
        {
            get
            {
                if (this.lfmArtist == null) return string.Empty;

                return this.lfmArtist.LargestImage();
            }
        }

        public string ArtistName
        {
            get
            {
                if (this.lfmArtist == null) return string.Empty;

                return this.lfmArtist.Name;
            }
        }

        public string Url
        {
            get
            {
                if (this.lfmArtist == null) return string.Empty;

                return this.lfmArtist.Url;
            }
        }

        public string UrlText
        {
            get
            {
                if (this.Biography == null) return string.Empty;

                Regex regex = new Regex(@"(>.*<\/a>)");
                Match match = regex.Match(this.Biography.Content);

                if (match.Success)
                {
                    return match.Groups[0].Value.Replace("</a>", "").Replace(">", "");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public LastFmBiography Biography
        {
            get
            {
                if (this.lfmArtist == null) return null;

                return this.lfmArtist.Biography;
            }
        }

        public string CleanedBiographyContent
        {
            get
            {
                if (this.Biography == null) return string.Empty;

                // Removes the URL from the Biography content
                string cleanedBiography = Regex.Replace(this.Biography.Content, @"(<a.*$)", "").Trim();
                return cleanedBiography;
            }
        }
        #endregion

        #region Private
        private void FillSimilarArtists()
        {
            if (lfmArtist != null && lfmArtist.SimilarArtists != null && lfmArtist.SimilarArtists.Count > 0)
            {

                var localSimilarArtists = new ObservableCollection<SimilarArtistViewModel>();

                foreach (LastFmArtist similarArtist in lfmArtist.SimilarArtists)
                {
                    localSimilarArtists.Add(new SimilarArtistViewModel { Name = similarArtist.Name, Url = similarArtist.Url });
                }

                this.SimilarArtists = localSimilarArtists;
            }
        }
        #endregion
    }
}
