using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Audio;
using Dopamine.Core.Settings;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Equalizer
{
    public class EqualizerService : IEqualizerService
    {
        #region Variables
        private string equalizerSubDirectory = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.EqualizerSubDirectory);
        #endregion

        #region Construction
        public EqualizerService()
        {
            // Initialize the Equalizer directory
            // ----------------------------------
            // If the Equalizer subdirectory doesn't exist, create it
            if (!Directory.Exists(this.equalizerSubDirectory))
            {
                Directory.CreateDirectory(Path.Combine(this.equalizerSubDirectory));
            }
        }
        #endregion

        #region IEqualizerService
        public async Task<List<EqualizerPreset>> GetEqualizerPresetsAsync()
        {
            var equalizerPresets = new List<EqualizerPreset>();

            await Task.Run(() =>
            {

            });

            return equalizerPresets;
        }
        #endregion

        private async Task GetBuiltInEqualizerPresetsAsync()
        {
            var builtinEqualizerPresets = new List<EqualizerPreset>();

            await Task.Run(() =>
            {
                string builtinEqualizerSubDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationPaths.EqualizerSubDirectory);

                var dirInfo = new DirectoryInfo(builtinEqualizerSubDirectory);

                foreach (FileInfo fileInfo in dirInfo.GetFiles("*" + FileFormats.EQUALIZERPRESET))
                {

                }
            });
        }
    }
}
