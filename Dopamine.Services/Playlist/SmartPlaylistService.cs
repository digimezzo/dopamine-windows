using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Dopamine.Services.Playlist
{
    public class SmartPlaylistService : PlaylistServiceBase, ISmartPlaylistService
    {
        public SmartPlaylistService() : base()
        {
        }

        public async Task<IList<string>> GetSmartPlaylistsAsync()
        {
            var playlists = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    var di = new DirectoryInfo(this.PlaylistFolder);
                    var fi = di.GetFiles("*" + FileFormats.DSPL, SearchOption.TopDirectoryOnly);

                    foreach (FileInfo f in fi)
                    {
                        playlists.Add(Path.GetFileNameWithoutExtension(f.FullName));
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
