using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Services.Playlist
{
    public class SmartPlaylistService : PlaylistServiceBase, ISmartPlaylistService
    {
        public SmartPlaylistService() : base()
        {
        }

        private string GetSmartPlaylistName(string smartPlaylistPath)
        {
            string name = string.Empty;

            try
            {
                XDocument xdoc = XDocument.Load(smartPlaylistPath);

                XElement nameElement = (from t in xdoc.Element("smartplaylist").Elements("name")
                                        select t).FirstOrDefault();

                name = nameElement.Value;
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not get name for smart playlist '{smartPlaylistPath}'. Exception: {ex.Message}");
            }

            return name;
        }

        public async Task<IList<PlaylistViewModel>> GetSmartPlaylistsAsync()
        {
            IList<PlaylistViewModel> playlists = new List<PlaylistViewModel>();

            await Task.Run(() =>
            {
                try
                {
                    var di = new DirectoryInfo(this.PlaylistFolder);
                    var fi = di.GetFiles("*" + FileFormats.DSPL, SearchOption.TopDirectoryOnly);

                    foreach (FileInfo f in fi)
                    {
                        string name = this.GetSmartPlaylistName(f.FullName);

                        if (!string.IsNullOrEmpty(name))
                        {
                            playlists.Add(new PlaylistViewModel(name, Path.GetFileNameWithoutExtension(f.FullName)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting smart playlists. Exception: {0}", ex.Message);
                }
            });

            return playlists;
        }
    }
}
