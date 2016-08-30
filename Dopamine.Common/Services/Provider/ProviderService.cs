using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Common.Services.Provider
{
    public class ProviderService : IProviderService
    {
        #region Variables
        private string providersXmlPath;
        private XDocument providersDocument;
        #endregion

        #region Construction
        public ProviderService()
        {
            this.providersXmlPath = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, "Providers.xml");

            // Create the XML containing the Providers
            this.CreateProvidersXml();

            // Load the XML containing the Providers
            this.LoadProvidersXml();
        }
        #endregion

        #region Private
        private void CreateProvidersXml()
        {
            // Only create this file if it doesn't yet exist. That allows the user to provide 
            // custom providers, without overwriting them the next time the application loads.
            if (!System.IO.File.Exists(this.providersXmlPath))
            {
                XDocument providersDocument = XDocument.Parse(
               "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
               "<Providers>" +
               "<SearchProviders>" +
               "<SearchProvider>" +
               "<Name>Video (Youtube)</Name>" +
               "<Url>https://www.youtube.com/results?search_query=</Url>" +
               "<Separator>+</Separator>" +
               "</SearchProvider>" +
               "<SearchProvider>" +
               "<Name>Lyrics (Musixmatch)</Name>" +
               "<Url>https://www.musixmatch.com/search/</Url>" +
               "<Separator>%20</Separator>" +
               "</SearchProvider>" +
               "</SearchProviders>" +
               "</Providers>");

                providersDocument.Save(this.providersXmlPath);
            }
        }

        private void LoadProvidersXml()
        {
            if (this.providersDocument == null)
            {
                try
                {
                    providersDocument = XDocument.Load(this.providersXmlPath);
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not load providers XML. Exception: {0}", ex.Message);
                }

            }
        }
        #endregion

        #region IProviderService
        public async Task<List<SearchProvider>> GetSearchProvidersAsync()
        {
            var providers = new List<SearchProvider>();

            await Task.Run(() =>
            {
                try
                {
                    if (this.providersDocument != null)
                    {
                        providers = (from t in this.providersDocument.Element("Providers").Elements("SearchProviders")
                                     from p in t.Elements("SearchProvider")
                                     from n in p.Elements("Name")
                                     from u in p.Elements("Url")
                                     from s in p.Elements("Separator")
                                     select new SearchProvider
                                     {
                                         Name = n.Value,
                                         Url = u.Value,
                                         Separator = s.Value
                                     }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not load search providers. Exception: {0}", ex.Message);
                }

            });

            return providers;
        }

        public void SearchOnline(string providerName, string[] searchArguments)
        {
            var provider = (from t in this.providersDocument.Element("Providers").Elements("SearchProviders")
                                 from p in t.Elements("SearchProvider")
                                 from n in p.Elements("Name")
                                 from u in p.Elements("Url")
                                 from s in p.Elements("Separator")
                                 where n.Value == providerName
                                 select new SearchProvider
                                 {
                                     Name = n.Value,
                                     Url = u.Value,
                                     Separator = s.Value
                                 }).FirstOrDefault();

            try
            {
                string url = provider.Url + string.Join(provider.Separator, searchArguments);
                Actions.TryOpenLink(url.Replace("&", provider.Separator)); // Because Youtube forgets the part of he URL that comes after "&"
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not search online. Exception: {0}", ex.Message);
            }
        }
        #endregion
    }
}
