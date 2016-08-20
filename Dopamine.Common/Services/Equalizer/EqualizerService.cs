using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Equalizer
{
    public class EqualizerService : IEqualizerService
    {
        #region Variables
        private string equalizerSubDirectory = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.EqualizerSubDirectory);
        private List<EqualizerPreset> presets;
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
        public async Task<EqualizerPreset> GetSelectedPresetAsync()
        {
            if(this.presets == null) await this.GetPresetsAsync();

            string selectedPresetName = XmlSettingsClient.Instance.Get<string>("Equalizer", "SelectedPreset");
            EqualizerPreset selectedPreset = this.presets.Select((p) => p).Where((p) => p.Name == selectedPresetName).FirstOrDefault();

            if(selectedPreset == null)
            {
                selectedPreset = new EqualizerPreset(Defaults.ManualPresetName, false);
            }

            return selectedPreset;
        }

        public async Task<List<EqualizerPreset>> GetPresetsAsync()
        {
            this.presets = new List<EqualizerPreset>();

            // First, get the builtin equalizer presets
            this.presets = await this.GetBuiltInPresetsAsync();

            // Then, get custom equalizer presets
            var customPresets = await this.GetCustomEqualizerPresetsAsync();

            foreach (var preset in customPresets)
            {
                // Give priority to built-in presets. If the user messed up and created a custom preset 
                // file which has the same name as a built-in preset file, the custom file is ignored.
                if (!this.presets.Contains(preset)) this.presets.Add(preset);
            }

            // Sort the presets
            this.presets = this.presets.OrderBy((p) => p.Name.ToLower()).ToList();

            // Insert manual preset in first position
            var manualPreset = new EqualizerPreset(Defaults.ManualPresetName, false);
            manualPreset.Load(ArrayUtils.ConvertArray(XmlSettingsClient.Instance.Get<string>("Equalizer", "ManualPreset").Split(';')));
            this.presets.Insert(0, manualPreset);

            return this.presets;
        }
        #endregion

        #region Private
        private async Task<List<EqualizerPreset>> GetBuiltInPresetsAsync()
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
                var bandValuesList = new List<double>();

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    double value;
                    if (double.TryParse(line, out value)) bandValuesList.Add(value);
                }

                preset.Load(bandValuesList.ToArray());
            }

            return preset;
        }
        #endregion
    }
}
