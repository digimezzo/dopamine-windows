using Dopamine.Core.Settings;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Common.Services.Provider
{
    public class ProviderService : IProviderService
    {
        #region Variables
        private string providersXmlFile;
        #endregion

        #region Construction
        public ProviderService()
        {
            // Create the XML containing the Providers
            this.CreateProvidersXml();
        }
        #endregion

        #region Private
        private void CreateProvidersXml()
        {
            XDocument providersDocument = XDocument.Parse(
                "<Providers>" +
                "<VideoProviders>" +
                "<VideoProvider>" +
                "<Name>Youtube</Name>" +
                "<Url>https://www.youtube.com/results?search_query=</Url>" +
                "<Separator>+</Separator>" +
                "</VideoProvider>" +
                "</VideoProviders>" +
                "</Providers>");

            this.providersXmlFile = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, "Providers.xml");

            providersDocument.Save(this.providersXmlFile);
        }
        #endregion

        #region IProviderService
        public async Task<List<VideoProvider>> GetVideoProvidersAsync()
        {
            var videoProviders = new List<VideoProvider>();

            await Task.Run(() =>
            {
                // TODO: implement
            });

            return videoProviders;
        }
        #endregion
    }
}
