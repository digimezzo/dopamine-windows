using Dopamine.Core.API.Lastfm;
using Dopamine.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.Api.Lastfm
{
    public static class ArtistApi
    {
        /// <summary>
        /// Gets artist information
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        public static async Task<LastFmArtist> GetInfo(string artist, bool autoCorrect, string languageCode)
        {
            string method = "artist.getInfo";

            var parameters = new Dictionary<string, string>();

            parameters.Add("method", method);
            if (!string.IsNullOrEmpty(languageCode)) parameters.Add("lang", languageCode);
            parameters.Add("artist", artist);
            parameters.Add("autocorrect", autoCorrect ? "1" : "0"); // 1 = transform misspelled artist names into correct artist names, returning the correct version instead. The corrected artist name will be returned in the response.
            parameters.Add("api_key", SensitiveInformation.LastfmApiKey);

            string result = await WebApi.PerformGetRequestAsync(method, parameters, false);

            var lfmArtist = new LastFmArtist();

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/artist.getInfo
                var resultXml = XDocument.Parse(result);

                // Name
                lfmArtist.Name = (from t in resultXml.Element("lfm").Element("artist").Elements("name")
                                  select t.Value).FirstOrDefault();

                // Url
                lfmArtist.Url = (from t in resultXml.Element("lfm").Element("artist").Elements("url")
                                 select t.Value).FirstOrDefault();

                // ImageSmall
                lfmArtist.ImageSmall = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                        where t.Attribute("size").Value == "small"
                                        select t.Value).FirstOrDefault();

                // ImageMedium
                lfmArtist.ImageMedium = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                         where t.Attribute("size").Value == "medium"
                                         select t.Value).FirstOrDefault();

                // ImageLarge
                lfmArtist.ImageLarge = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                        where t.Attribute("size").Value == "large"
                                        select t.Value).FirstOrDefault();

                // ImageExtraLarge
                lfmArtist.ImageExtraLarge = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                             where t.Attribute("size").Value == "extralarge"
                                             select t.Value).FirstOrDefault();

                // ImageMega
                lfmArtist.ImageMega = (from t in resultXml.Element("lfm").Element("artist").Elements("image")
                                       where t.Attribute("size").Value == "mega"
                                       select t.Value).FirstOrDefault();

                // SimilarArtists
                lfmArtist.SimilarArtists = (from t in resultXml.Element("lfm").Element("artist").Element("similar").Elements("artist")
                                            select new LastFmArtist
                                            {
                                                Name = t.Descendants("name").FirstOrDefault().Value,
                                                Url = t.Descendants("url").FirstOrDefault().Value,
                                                ImageSmall = t.Descendants("image").Where((i) => i.Attribute("size").Value == "small").FirstOrDefault().Value,
                                                ImageMedium = t.Descendants("image").Where((i) => i.Attribute("size").Value == "medium").FirstOrDefault().Value,
                                                ImageLarge = t.Descendants("image").Where((i) => i.Attribute("size").Value == "large").FirstOrDefault().Value,
                                                ImageExtraLarge = t.Descendants("image").Where((i) => i.Attribute("size").Value == "extralarge").FirstOrDefault().Value,
                                                ImageMega = t.Descendants("image").Where((i) => i.Attribute("size").Value == "mega").FirstOrDefault().Value
                                            }).ToList();

                // Biography
                lfmArtist.Biography = (from t in resultXml.Element("lfm").Element("artist").Elements("bio")
                                       select new LastFmBiography
                                       {
                                           Published = t.Descendants("published").FirstOrDefault().Value,
                                           Summary = t.Descendants("summary").FirstOrDefault().Value,
                                           Content = t.Descendants("content").FirstOrDefault().Value
                                       }).FirstOrDefault();
            }

            return lfmArtist;
        }
    }
}
