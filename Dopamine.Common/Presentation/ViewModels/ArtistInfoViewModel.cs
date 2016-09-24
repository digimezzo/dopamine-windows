using Dopamine.Core.API.Lastfm;
using Prism.Mvvm;
using System.Text.RegularExpressions;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class ArtistInfoViewModel : BindableBase
    {
        #region Variables
        private LastFmArtist lfmArtist;
        #endregion

        #region Properties
        public LastFmArtist LfmArtist
        {
            get { return this.lfmArtist; }
            set
            {
                SetProperty<LastFmArtist>(ref this.lfmArtist, value);
                OnPropertyChanged(() => this.Biography);
                OnPropertyChanged(() => this.Url);
                OnPropertyChanged(() => this.CleanedBiographyContent);
                OnPropertyChanged(() => this.UrlText);
                OnPropertyChanged(() => this.ArtistName);
                OnPropertyChanged(() => this.Image);
            }
        }

        public string Image
        {
            get
            {
                if (!string.IsNullOrEmpty(this.lfmArtist.ImageMega))
                {
                    return this.lfmArtist.ImageMega;
                }
                else if (!string.IsNullOrEmpty(this.lfmArtist.ImageExtraLarge))
                {
                    return this.lfmArtist.ImageExtraLarge;
                }
                else if (!string.IsNullOrEmpty(this.lfmArtist.ImageLarge))
                {
                    return this.lfmArtist.ImageLarge;
                }
                else if (!string.IsNullOrEmpty(this.lfmArtist.ImageMedium))
                {
                    return this.lfmArtist.ImageMedium;
                }
                else if (!string.IsNullOrEmpty(this.lfmArtist.ImageSmall))
                {
                    return this.lfmArtist.ImageSmall;
                }else
                {
                    return string.Empty;
                }
            }
        }

        public string ArtistName
        {
            get { return this.lfmArtist.Name; }
        }

        public string Url
        {
            get { return this.lfmArtist.Url; }
        }

        public string UrlText
        {
            get
            {
                Regex regex = new Regex(@"(>.*<\/a>)");
                Match match = regex.Match(this.lfmArtist.Biography.Content);

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
                return this.lfmArtist.Biography;
            }
        }

        public string CleanedBiographyContent
        {
            get
            {
                // Removes the URL from the Biography content
                string cleanedBiography = Regex.Replace(this.lfmArtist.Biography.Content, @"(<a.*$)", "").Trim();
                return cleanedBiography;
            }
        }
        #endregion
    }
}
