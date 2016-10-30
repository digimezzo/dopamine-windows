using Dopamine.Core.API.Lastfm;
using Dopamine.Core.Base;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.Api.Lastfm
{
    public static class AlbumApi
    {
        /// <summary>
        /// Gets album information
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public static async Task<LastFmAlbum> GetInfo(string artist, string album, bool autoCorrect, string languageCode)
        {
            string method = "album.getInfo";

            var parameters = new Dictionary<string, string>();

            parameters.Add("method", method);
            if (!string.IsNullOrEmpty(languageCode)) parameters.Add("lang", languageCode);
            parameters.Add("artist", artist);
            parameters.Add("album", album);
            parameters.Add("autocorrect", autoCorrect ? "1" : "0"); // 1 = transform misspelled artist names into correct artist names, returning the correct version instead. The corrected artist name will be returned in the response.
            parameters.Add("api_key", SensitiveInformation.LastfmApiKey);

            string result = await WebApi.PerformGetRequestAsync(method, parameters, false);

            var lfmAlbum = new LastFmAlbum();

            if (!string.IsNullOrEmpty(result))
            {
                // http://www.last.fm/api/show/album.getInfo
                var resultXml = XDocument.Parse(result);

                // Artist
                lfmAlbum.Artist = (from t in resultXml.Element("lfm").Element("album").Elements("artist")
                                   select t.Value).FirstOrDefault();

                // Name
                lfmAlbum.Name = (from t in resultXml.Element("lfm").Element("album").Elements("name")
                                 select t.Value).FirstOrDefault();

                // Url
                lfmAlbum.Url = (from t in resultXml.Element("lfm").Element("album").Elements("url")
                                select t.Value).FirstOrDefault();

                // ImageSmall
                lfmAlbum.ImageSmall = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                       where t.Attribute("size").Value == "small"
                                       select t.Value).FirstOrDefault();

                // ImageMedium
                lfmAlbum.ImageMedium = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                        where t.Attribute("size").Value == "medium"
                                        select t.Value).FirstOrDefault();

                // ImageLarge
                lfmAlbum.ImageLarge = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                       where t.Attribute("size").Value == "large"
                                       select t.Value).FirstOrDefault();

                // ImageExtraLarge
                lfmAlbum.ImageExtraLarge = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                            where t.Attribute("size").Value == "extralarge"
                                            select t.Value).FirstOrDefault();

                // ImageMega
                lfmAlbum.ImageMega = (from t in resultXml.Element("lfm").Element("album").Elements("image")
                                      where t.Attribute("size").Value == "mega"
                                      select t.Value).FirstOrDefault();
            }
            return lfmAlbum;
        }
    }
}
