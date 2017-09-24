using Dopamine.Common.Api.Lastfm;
using Prism.Mvvm;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using Dopamine.Common.Services.Cache;
using System.Threading.Tasks;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ArtistInfoViewModel : BindableBase
    {
        #region Variables
        private Artist lfmArtist;
        private ObservableCollection<SimilarArtistViewModel> similarArtists;
        private ICacheService cacheService;
        private string image;
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
            get { return !string.IsNullOrEmpty(this.image); }
        }

        public ObservableCollection<SimilarArtistViewModel> SimilarArtists
        {
            get { return this.similarArtists; }
            set { SetProperty<ObservableCollection<SimilarArtistViewModel>>(ref this.similarArtists, value); }
        }

        public async Task SetLastFmArtistAsync(Artist lfmArtist)
        {
            this.lfmArtist = lfmArtist;

            OnPropertyChanged(() => this.ArtistName);
            OnPropertyChanged(() => this.Biography);
            OnPropertyChanged(() => this.HasBiography);
            OnPropertyChanged(() => this.CleanedBiographyContent);
            OnPropertyChanged(() => this.Url);
            OnPropertyChanged(() => this.UrlText);

            await this.FillSimilarArtistsAsync();
            await this.FillImageAsync();
        }

        public string Image
        {
            get
            {
                return this.image;
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

        public Biography Biography
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

        #region Construction
        public ArtistInfoViewModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }
        #endregion

        #region Private
        private async Task FillSimilarArtistsAsync()
        {
            if (this.lfmArtist != null && this.lfmArtist.SimilarArtists != null && this.lfmArtist.SimilarArtists.Count > 0)
            {
                await Task.Run(() =>
                {
                    var localSimilarArtists = new ObservableCollection<SimilarArtistViewModel>();

                    foreach (Artist similarArtist in this.lfmArtist.SimilarArtists)
                    {
                        localSimilarArtists.Add(new SimilarArtistViewModel { Name = similarArtist.Name, Url = similarArtist.Url, ImageUrl = similarArtist.LargestImage() });
                    }

                    this.SimilarArtists = localSimilarArtists;
                });
            }

            OnPropertyChanged(() => this.SimilarArtists);
            OnPropertyChanged(() => this.HasSimilarArtists);
        }

        private async Task FillImageAsync()
        {
            if (this.lfmArtist == null || string.IsNullOrEmpty(this.lfmArtist.LargestImage())) return;

            this.image = await this.cacheService.DownloadFileToTemporaryCacheAsync(new Uri(this.lfmArtist.LargestImage()));

            OnPropertyChanged(() => this.Image);
            OnPropertyChanged(() => this.HasImage);
        }
        #endregion
    }
}
