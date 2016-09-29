using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Core.API.Lastfm
{
    public class LastFmArtist
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string ImageSmall { get; set; }
        public string ImageMedium { get; set; }
        public string ImageLarge { get; set; }
        public string ImageExtraLarge { get; set; }
        public string ImageMega { get; set; }
        public List<LastFmArtist> SimilarArtists { get; set; }
        public LastFmBiography Biography { get; set; }
    }
}
