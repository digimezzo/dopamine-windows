using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using System;
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

            // First, get the builtin equalizer presets
            equalizerPresets = await this.GetBuiltInEqualizerPresetsAsync();

            // Then, get custom equalizer presets
            var customPresets = await this.GetCustomEqualizerPresetsAsync();

            foreach (var preset in customPresets)
            {
                // Give priority to built-in presets. If the user messed up and created a custom preset 
                // file which has the same name as a built-in preset file, the custom file is ignored.
                if (!equalizerPresets.Contains(preset)) equalizerPresets.Add(preset);
            }

            return equalizerPresets;
        }
        #endregion

        #region Private
        private async Task<List<EqualizerPreset>> GetBuiltInEqualizerPresetsAsync()
        {
            var builtinEqualizerPresets = new List<EqualizerPreset>();

            await Task.Run(() =>
            {
                string builtinPresetSubDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationPaths.EqualizerSubDirectory);

                var dirInfo = new DirectoryInfo(builtinPresetSubDirectory);

                foreach (FileInfo fileInfo in dirInfo.GetFiles("*" + FileFormats.EQUALIZERPRESET))
                {
                    try
                    {
                        builtinEqualizerPresets.Add(this.CreatePresetFromFile(fileInfo.FullName, true));
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Could not load built-in preset from file '{0}'. Exception: {1}", fileInfo.FullName, ex.Message);
                    }
                }
            });

            return builtinEqualizerPresets;
        }

        private async Task<List<EqualizerPreset>> GetCustomEqualizerPresetsAsync()
        {
            var customEqualizerPresets = new List<EqualizerPreset>();

            await Task.Run(() =>
            {
                var dirInfo = new DirectoryInfo(this.equalizerSubDirectory);

                foreach (FileInfo fileInfo in dirInfo.GetFiles("*" + FileFormats.EQUALIZERPRESET))
                {
                    try
                    {
                        customEqualizerPresets.Add(this.CreatePresetFromFile(fileInfo.FullName, true));
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Could not load custom preset from file '{0}'. Exception: {1}", fileInfo.FullName, ex.Message);
                    }
                }
            });

            return customEqualizerPresets;
        }

        private EqualizerPreset CreatePresetFromFile(string filename, bool isRemovable)
        {
            var preset = new EqualizerPreset(Path.GetFileNameWithoutExtension(filename), isRemovable);

            using (var reader = new StreamReader(filename))
            {
                int lineNumber = 0;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    double value;
                    if (double.TryParse(line, out value)) preset.SetBandValue(lineNumber, value);
                    
                    lineNumber++;
                }
            }

            return preset;
        }
        #endregion
    }
}
