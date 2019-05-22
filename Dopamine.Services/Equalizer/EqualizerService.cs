using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Utils;
using Dopamine.Services.Equalizer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dopamine.Services.Equalizer
{
    public class EqualizerService : IEqualizerService
    {
        private string equalizerSubDirectory;

        public EqualizerService()
        {
            // Initialize the Equalizer directory
            // ----------------------------------
            this.equalizerSubDirectory = Path.Combine(SettingsClient.ApplicationFolder(), ApplicationPaths.EqualizerFolder);

            // If the Equalizer subdirectory doesn't exist, create it
            if (!Directory.Exists(this.equalizerSubDirectory))
            {
                Directory.CreateDirectory(Path.Combine(this.equalizerSubDirectory));
            }
        }
     
        public async Task<EqualizerPreset> GetSelectedPresetAsync()
        {
            var presets = await this.GetPresetsAsync();

            string selectedPresetName = SettingsClient.Get<string>("Equalizer", "SelectedPreset");
            EqualizerPreset selectedPreset = presets.Select((p) => p).Where((p) => p.Name == selectedPresetName).FirstOrDefault();

            if(selectedPreset == null)
            {
                selectedPreset = new EqualizerPreset(Defaults.ManualPresetName, false);
            }

            return selectedPreset;
        }

        public async Task<List<EqualizerPreset>> GetPresetsAsync()
        {
            var presets = new List<EqualizerPreset>();

            // First, get the built-in equalizer presets
            presets = await this.GetBuiltInPresetsAsync();

            // Then, get custom equalizer presets
            var customPresets = await this.GetCustomEqualizerPresetsAsync();

            foreach (var preset in customPresets)
            {
                // Give priority to built-in presets. If the user messed up and created a custom preset 
                // file which has the same name as a built-in preset file, the custom file is ignored.
                if (!presets.Contains(preset)) presets.Add(preset);
            }

            // Sort the presets
            presets = presets.OrderBy((p) => p.Name.ToLower()).ToList();

            // Insert manual preset in first position
            var manualPreset = new EqualizerPreset(Defaults.ManualPresetName, false);
            manualPreset.Load(ArrayUtils.ConvertArray(SettingsClient.Get<string>("Equalizer", "ManualPreset").Split(';')));
            presets.Insert(0, manualPreset);

            return presets;
        }
      
        private async Task<List<EqualizerPreset>> GetBuiltInPresetsAsync()
        {
            var builtinEqualizerPresets = new List<EqualizerPreset>();

            await Task.Run(() =>
            {
                string builtinPresetSubDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationPaths.EqualizerFolder);

                var dirInfo = new DirectoryInfo(builtinPresetSubDirectory);

                foreach (FileInfo fileInfo in dirInfo.GetFiles("*" + FileFormats.DEQ))
                {
                    try
                    {
                        builtinEqualizerPresets.Add(this.CreatePresetFromFile(fileInfo.FullName, false));
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not load built-in preset from file '{0}'. Exception: {1}", fileInfo.FullName, ex.Message);
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

                foreach (FileInfo fileInfo in dirInfo.GetFiles("*" + FileFormats.DEQ))
                {
                    try
                    {
                        customEqualizerPresets.Add(this.CreatePresetFromFile(fileInfo.FullName, true));
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not load custom preset from file '{0}'. Exception: {1}", fileInfo.FullName, ex.Message);
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
                var bandValuesList = new List<double>();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    double value;
                    if (double.TryParse(line, NumberStyles.Number, CultureInfo.InvariantCulture, out value)) bandValuesList.Add(value);
                }

                preset.Load(bandValuesList.ToArray());
            }

            return preset;
        }
    }
}
