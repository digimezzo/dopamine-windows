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

        #region Events
        public event EventHandler SearchProvidersChanged = delegate { };
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
               "<Id>e76c9f60-ef0e-4468-b47a-2889810fde85</Id>" +
               "<Name>Video (Youtube)</Name>" +
               "<Url>https://www.youtube.com/results?search_query=</Url>" +
               "<Separator>+</Separator>" +
               "</SearchProvider>" +
               "<SearchProvider>" +
               "<Id>0d08bb4d-68b1-4c19-b952-e76d06d198fa</Id>" +
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
                                     from i in p.Elements("Id")
                                     from n in p.Elements("Name")
                                     from u in p.Elements("Url")
                                     from s in p.Elements("Separator")
                                     select new SearchProvider
                                     {
                                         Id = i.Value,
                                         Name = n.Value,
                                         Url = u.Value,
                                         Separator = s.Value
                                     }).Distinct().ToList();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not load search providers. Exception: {0}", ex.Message);
                }

            });

            return providers.OrderBy((p) => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public void SearchOnline(string id, string[] searchArguments)
        {
            var provider = (from t in this.providersDocument.Element("Providers").Elements("SearchProviders")
                            from p in t.Elements("SearchProvider")
                            from i in p.Elements("Id")
                            from n in p.Elements("Name")
                            from u in p.Elements("Url")
                            from s in p.Elements("Separator")
                            where i.Value == id
                            select new SearchProvider
                            {
                                Id = i.Value,
                                Name = n.Value,
                                Url = u.Value,
                                Separator = !string.IsNullOrWhiteSpace(s.Value) ? s.Value : "%20"
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

        public bool RemoveSearchProvider(SearchProvider provider)
        {
            bool returnValue = false;

            XElement providerElementToRemove = (from t in this.providersDocument.Element("Providers").Elements("SearchProviders")
                                                from p in t.Elements("SearchProvider")
                                                from i in p.Elements("Id")
                                                where i.Value == provider.Id
                                                select p).FirstOrDefault();

            if (providerElementToRemove != null)
            {
                try
                {
                    providerElementToRemove.Remove();
                    this.providersDocument.Save(this.providersXmlPath);
                    returnValue = true;
                    this.SearchProvidersChanged(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not remove search provider. Exception: {0}", ex.Message);
                }
            }

            return returnValue;
        }

        public bool UpdateSearchProvider(SearchProvider provider)
        {
            bool returnValue = false;

            XElement providerElementToUpdate = (from t in this.providersDocument.Element("Providers").Elements("SearchProviders")
                                                from p in t.Elements("SearchProvider")
                                                from i in p.Elements("Id")
                                                where i.Value == provider.Id
                                                select p).FirstOrDefault();

            try
            {
                if (providerElementToUpdate != null)
                {
                    providerElementToUpdate.SetElementValue("Name", provider.Name);
                    providerElementToUpdate.SetElementValue("Url", provider.Url);
                    providerElementToUpdate.SetElementValue("Separator", provider.Separator);

                    this.providersDocument.Save(this.providersXmlPath);
                    returnValue = true;
                    this.SearchProvidersChanged(this, new EventArgs());
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(provider.Name) && !string.IsNullOrWhiteSpace(provider.Url))
                    {
                        XElement searchProvider = new XElement("SearchProvider");
                        searchProvider.SetElementValue("Id", Guid.NewGuid().ToString());
                        searchProvider.SetElementValue("Name", provider.Name);
                        searchProvider.SetElementValue("Url", provider.Url);
                        searchProvider.SetElementValue("Separator", provider.Separator != null ? provider.Separator : string.Empty);

                        this.providersDocument.Element("Providers").Element("SearchProviders").Add(searchProvider);

                        this.providersDocument.Save(this.providersXmlPath);
                        returnValue = true;
                        this.SearchProvidersChanged(this, new EventArgs());
                    }else
                    {
                        LogClient.Instance.Logger.Error("Parameters 'Name' and 'Url' must be filled in for the search provider, 'Separator' is optional.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not update search provider. Exception: {0}", ex.Message);
            }

            return returnValue;
        }
        #endregion
    }
}
