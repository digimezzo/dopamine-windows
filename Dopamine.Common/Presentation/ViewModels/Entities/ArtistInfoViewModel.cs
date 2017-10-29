using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Api.Lastfm;
using Dopamine.Common.Services.Cache;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dopamine.Common.Presentation.ViewModels.Entities
{
    public class ArtistInfoViewModel : BindableBase
    {
        private Artist lfmArtist;
        private ObservableCollection<SimilarArtistViewModel> similarArtists;
        private ICacheService cacheService;
        private string image;

        public DelegateCommand<string> OpenLinkCommand { get; set; }
       
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

            RaisePropertyChanged(nameof(this.ArtistName));
            RaisePropertyChanged(nameof(this.Biography));
            RaisePropertyChanged(nameof(this.HasBiography));
            RaisePropertyChanged(nameof(this.CleanedBiographyContent));
            RaisePropertyChanged(nameof(this.Url));
            RaisePropertyChanged(nameof(this.UrlText));

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

        public ArtistInfoViewModel(ICacheService cacheService)
        {
            this.cacheService = cacheService;

            this.OpenLinkCommand = new DelegateCommand<string>((url) =>
            {
                try
                {
                    Actions.TryOpenLink(url);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open link {0}. Exception: {1}", url, ex.Message);
                }
            });
        }

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

            RaisePropertyChanged(nameof(this.SimilarArtists));
            RaisePropertyChanged(nameof(this.HasSimilarArtists));
        }

        private async Task FillImageAsync()
        {
            if (this.lfmArtist == null || string.IsNullOrEmpty(this.lfmArtist.LargestImage())) return;

            this.image = await this.cacheService.DownloadFileToTemporaryCacheAsync(new Uri(this.lfmArtist.LargestImage()));

            RaisePropertyChanged(nameof(this.Image));
            RaisePropertyChanged(nameof(this.HasImage));
        }
    }
}
