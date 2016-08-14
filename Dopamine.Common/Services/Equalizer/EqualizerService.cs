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
        private string equalizerSubDirectory = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.EqualizerSubDirectory);
        #endregion

        #region Properties
        public EqualizerPreset Preset
        {
            get
            {
                if (this.preset == null)
                {
                    this.preset = new EqualizerPreset(XmlSettingsClient.Instance.Get<string>("EqualizerPreset", "Name"), 10);

                    try
                    {
                        this.preset.Load(ArrayUtils.ConvertArray(XmlSettingsClient.Instance.Get<string>("EqualizerPreset", "Bands").Split(';')));
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("An exception occured while loading Equalizer bands from the settings. Exception: {0}", ex.Message);
                        this.preset.LoadDefault();
                    }
                }

                return this.preset;
            }
            set { this.preset = value; }
        }
        #endregion

        #region Events
        public event EqualizerPresetChangedEventhandler EqualizerPresetChanged = delegate { };
        public event EqualizerBandChangedEventhandler EqualizerBandChanged = delegate { };
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
        public void SetEqualizerBand(int band, double value)
        {
            this.Preset.Bands[band] = value;
            this.EqualizerBandChanged(band,value);
            // Update settings
        }

        public void SetEqualizerPreset(EqualizerPreset preset)
        {
            this.Preset = preset;
            this.EqualizerPresetChanged(preset);
            // Update settings
        }

        public async Task<List<EqualizerPreset>> GetEqualizerPresetsAsync()
        {
            var equalizerPresets = new List<EqualizerPreset>();

            await Task.Run(() =>
            {

            });

            return equalizerPresets;
        }
        #endregion

        #region Private
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
        #endregion
    }
}
