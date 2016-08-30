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
               "<VideoProviders>" +
               "<VideoProvider>" +
               "<Name>Youtube</Name>" +
               "<Url>https://www.youtube.com/results?search_query=</Url>" +
               "<Separator>+</Separator>" +
               "</VideoProvider>" +
               "</VideoProviders>" +
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
                    LogClient.Instance.Logger.Error("Could not load Providers XML. Exception: {0}", ex.Message);
                }

            }
        }
        #endregion

        #region IProviderService
        public async Task<List<VideoProvider>> GetVideoProvidersAsync()
        {
            var videoProviders = new List<VideoProvider>();

            await Task.Run(() =>
            {
                try
                {
                    if (this.providersDocument != null)
                    {
                        videoProviders = (from v in this.providersDocument.Element("Providers").Elements("VideoProviders")
                                          from p in v.Elements("VideoProvider")
                                          from n in p.Elements("Name")
                                          from u in p.Elements("Url")
                                          from s in p.Elements("Separator")
                                          select new VideoProvider
                                          {
                                              Name = n.Value,
                                              Url = u.Value,
                                              Separator = s.Value
                                          }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("Could not load VideoProviders. Exception: {0}", ex.Message);
                }

            });

            return videoProviders;
        }

        public void SearchVideo(string providerName, string[] searchArguments)
        {
            var videoProvider = (from v in this.providersDocument.Element("Providers").Elements("VideoProviders")
                                 from p in v.Elements("VideoProvider")
                                 from n in p.Elements("Name")
                                 from u in p.Elements("Url")
                                 from s in p.Elements("Separator")
                                 where n.Value == providerName
                                 select new VideoProvider
                                 {
                                     Name = n.Value,
                                     Url = u.Value,
                                     Separator = s.Value
                                 }).FirstOrDefault();

            try
            {
                string url = videoProvider.Url + string.Join(videoProvider.Separator, searchArguments);
                Actions.TryOpenLink(url.Replace("&", videoProvider.Separator)); // Because Youtube forgets the part of he URL that comes after "&"
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not search for video. Exception: {0}", ex.Message);
            }
        }
        #endregion
    }
}
