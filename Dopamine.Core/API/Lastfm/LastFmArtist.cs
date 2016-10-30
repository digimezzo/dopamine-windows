using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Core.Api.Lastfm
{
    public class LastFmArtist
    {
        #region Properties
        public string Name { get; set; }
        public string Url { get; set; }
        public string ImageSmall { get; set; }
        public string ImageMedium { get; set; }
        public string ImageLarge { get; set; }
        public string ImageExtraLarge { get; set; }
        public string ImageMega { get; set; }
        public List<LastFmArtist> SimilarArtists { get; set; }
        public LastFmBiography Biography { get; set; }
        #endregion

        #region Public
        public string LargestImage()
        {
            if (!string.IsNullOrEmpty(this.ImageMega))
            {
                return this.ImageMega;
            }
            else if (!string.IsNullOrEmpty(this.ImageExtraLarge))
            {
                return this.ImageExtraLarge;
            }
            else if (!string.IsNullOrEmpty(this.ImageLarge))
            {
                return this.ImageLarge;
            }
            else if (!string.IsNullOrEmpty(this.ImageMedium))
            {
                return this.ImageMedium;
            }
            else if (!string.IsNullOrEmpty(this.ImageSmall))
            {
                return this.ImageSmall;
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
