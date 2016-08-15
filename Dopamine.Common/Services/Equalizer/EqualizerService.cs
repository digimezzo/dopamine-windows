using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
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
        private EqualizerPreset preset;
        private bool isEnabled;
        private string equalizerSubDirectory = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.EqualizerSubDirectory);
        #endregion

        #region Properties
        public bool IsEnabled
        {
            get
            {
                return this.isEnabled;
            }
            set
            {
                this.isEnabled = value;
                this.EqualizerIsEnabledChanged(value);
                XmlSettingsClient.Instance.Set<bool>("Equalizer", "IsEnabled", value);
            }
        }

        public EqualizerPreset Preset
        {
            get
            {
                return this.preset;
            }
        }
        #endregion

        #region Events
        public event EqualizerPresetChangedEventhandler EqualizerPresetChanged = delegate { };
        public event EqualizerBandChangedEventhandler EqualizerBandChanged = delegate { };
        public event EqualizerIsEnabledChangedEventHandler EqualizerIsEnabledChanged = delegate { };
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

            this.isEnabled = XmlSettingsClient.Instance.Get<bool>("Equalizer", "IsEnabled");
        }
        #endregion

        #region IEqualizerService
        public void SetEqualizerBand(int band, double value)
        {
            this.Preset.Bands[band] = value;
            this.EqualizerBandChanged(band, value);
            // TODO: update settings
        }

        public void SetEqualizerPreset(EqualizerPreset preset)
        {
            this.preset = preset;
            this.EqualizerPresetChanged(preset);
            // TODO: update settings
        }

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

            // Get the saved selected preset from the settings
            string savedSelectedPresetName = XmlSettingsClient.Instance.Get<string>("Equalizer", "SelectedPreset");

            foreach (EqualizerPreset preset in equalizerPresets)
            {
                if (preset.Name == savedSelectedPresetName)
                {
                    this.preset = preset;
                }
                break;
            }

            // The saved preset name was not found among the available presets:
            // Provide the manual preset fom the settings.
            if (this.Preset == null)
            {
                var manualPreset = new EqualizerPreset("%manual%", false);
                manualPreset.Load(ArrayUtils.ConvertArray(XmlSettingsClient.Instance.Get<string>("Equalizer", "ManualPreset").Split(';')));
                this.preset = manualPreset;
            }

            // Add Manual preset
            equalizerPresets.Insert(0, new EqualizerPreset("%manual%", false));

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
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line.Contains("="))
                    {
                        var pieces = line.Split('=');

                        if (pieces.Length == 2)
                        {
                            switch (pieces[0])
                            {
                                case "Band1":
                                    preset.Bands[0] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band2":
                                    preset.Bands[1] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band3":
                                    preset.Bands[2] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band4":
                                    preset.Bands[3] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band5":
                                    preset.Bands[4] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band6":
                                    preset.Bands[5] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band7":
                                    preset.Bands[6] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band8":
                                    preset.Bands[7] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band9":
                                    preset.Bands[8] = Convert.ToDouble(pieces[1]);
                                    break;
                                case "Band10":
                                    preset.Bands[9] = Convert.ToDouble(pieces[1]);
                                    break;
                            }
                        }
                    }
                }
            }

            return preset;
        }
        #endregion
    }
}
