using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.Equalizer;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dopamine.ControlsModule.ViewModels
{
    public class EqualizerControlViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private IEqualizerService equalizerService;
        private IDialogService dialogService;

        private ObservableCollection<EqualizerPreset> presets;
        private EqualizerPreset selectedPreset;
        private bool isEqualizerEnabled;
        #endregion

        #region Commands
        public DelegateCommand ResetCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        #endregion

        #region Properties
        public ObservableCollection<EqualizerPreset> Presets
        {
            get { return this.presets; }
            set
            {
                SetProperty<ObservableCollection<EqualizerPreset>>(ref this.presets, value);
            }
        }

        public EqualizerPreset SelectedPreset
        {
            get { return this.selectedPreset; }
            set
            {
                value.BandValueChanged -= SelectedPreset_BandValueChanged;
                if (value.Name == Defaults.ManualPresetName) value.DisplayName = ResourceUtils.GetStringResource("Language_Manual"); // Make sure the manual preset is translated
                SetProperty<EqualizerPreset>(ref this.selectedPreset, value);
                if (XmlSettingsClient.Instance.Get<string>("Equalizer", "SelectedPreset") != value.Name) XmlSettingsClient.Instance.Set<string>("Equalizer", "SelectedPreset", value.Name);
                this.playbackService.SwitchPreset(ref value); // Make sure that playbackService has a reference to the selected preset
                value.BandValueChanged += SelectedPreset_BandValueChanged;
            }
        }

        public bool IsEqualizerEnabled
        {
            get { return this.isEqualizerEnabled; }
            set
            {
                SetProperty<bool>(ref this.isEqualizerEnabled, value);
                XmlSettingsClient.Instance.Set<bool>("Equalizer", "IsEnabled", value);
                this.playbackService.SetIsEqualizerEnabled(value);
            }
        }
        #endregion

        #region Construction
        public EqualizerControlViewModel(IPlaybackService playbackService, IEqualizerService equalizerService, IDialogService dialogService)
        {
            // Variables
            this.playbackService = playbackService;
            this.equalizerService = equalizerService;
            this.dialogService = dialogService;

            this.IsEqualizerEnabled = XmlSettingsClient.Instance.Get<bool>("Equalizer", "IsEnabled");

            // Commands
            this.ResetCommand = new DelegateCommand(() =>
            {
                if (this.SelectedPreset.Name == Defaults.ManualPresetName)
                {
                    this.SelectedPreset.Reset();
                }
                else
                {
                    this.SelectedPreset = new EqualizerPreset(Defaults.ManualPresetName, false);
                }

                this.SaveManualPreset();
            });

            this.SaveCommand = new DelegateCommand(() => { this.SavePresetToFile(); });

            // Initialize
            this.InitializeAsync();
        }
        #endregion

        #region Private
        private async void InitializeAsync()
        {
            ObservableCollection<EqualizerPreset> localEqualizerPresets = new ObservableCollection<EqualizerPreset>();

            foreach (EqualizerPreset preset in await this.equalizerService.GetPresetsAsync())
            {
                if (preset.Name == Defaults.ManualPresetName) preset.DisplayName = ResourceUtils.GetStringResource("Language_Manual"); // Make sure the manual preset is translated
                localEqualizerPresets.Add(preset);
            }

            this.Presets = localEqualizerPresets;

            this.SelectedPreset = await this.equalizerService.GetSelectedPresetAsync(); // Don't use the SelectedPreset setter directly, because that saves SelectedPreset to the settings file.
        }

        private void SaveManualPreset()
        {
            // When the value of a slider changes, default to the manual preset the next time.
            XmlSettingsClient.Instance.Set<string>("Equalizer", "SelectedPreset", Defaults.ManualPresetName);

            // Also store the values for the manual preset
            XmlSettingsClient.Instance.Set<string>("Equalizer", "ManualPreset", this.SelectedPreset.ToValueString());

            // Set the selected preset to the manual preset
            var preset = new EqualizerPreset(Defaults.ManualPresetName, false) { Bands = this.SelectedPreset.Bands };
            preset.DisplayName = ResourceUtils.GetStringResource("Language_Manual"); // Make sure the manual preset is translated
            this.SelectedPreset = preset;
        }

        private void SavePresetToFile()
        {
            var showSaveDialog = true;

            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = string.Empty;
            dlg.DefaultExt = FileFormats.EQUALIZERPRESET;
            dlg.Filter = string.Concat(ResourceUtils.GetStringResource("Language_Equalizer_Presets"), " (", FileFormats.EQUALIZERPRESET, ")|*", FileFormats.EQUALIZERPRESET);
            dlg.InitialDirectory = System.IO.Path.Combine(LegacyPaths.AppData(), ProductInformation.ApplicationAssemblyName, ApplicationPaths.EqualizerSubDirectory);

            while (showSaveDialog)
            {
                if ((bool)dlg.ShowDialog())
                {
                    List<string> names = this.presets.Select((p) => p.Name.ToLower()).ToList();

                    if (names.Contains(System.IO.Path.GetFileNameWithoutExtension(dlg.FileName.ToLower())))
                    {
                        dlg.FileName = string.Empty;

                        this.dialogService.ShowNotification(
                                                0xe711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                ResourceUtils.GetStringResource("Language_Preset_Already_Taken"),
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                false,
                                                string.Empty);
                    }
                    else
                    {
                        showSaveDialog = false;

                        try
                        {
                            string[] lines = this.SelectedPreset.Bands.Select((b) => b.Value.ToString()).ToArray();
                            System.IO.File.WriteAllLines(dlg.FileName, lines);
                        }
                        catch (Exception ex)
                        {
                            LogClient.Instance.Logger.Error("An error occured while saving preset to file '{0}'. Exception: {1}", dlg.FileName, ex.Message);

                            this.dialogService.ShowNotification(
                                                0xe711,
                                                16,
                                                ResourceUtils.GetStringResource("Language_Error"),
                                                ResourceUtils.GetStringResource("Language_Error_While_Saving_Preset"),
                                                ResourceUtils.GetStringResource("Language_Ok"),
                                                true,
                                                ResourceUtils.GetStringResource("Language_Log_File"));

                        }
                        XmlSettingsClient.Instance.Set<string>("Equalizer", "SelectedPreset", System.IO.Path.GetFileNameWithoutExtension(dlg.FileName));
                        this.InitializeAsync();
                    }
                }
                else
                {
                    showSaveDialog = false; // Makes sure the dialog doesn't re-appear when pressing cancel
                }
            }
        }
        #endregion

        #region Event Handlers
        private void SelectedPreset_BandValueChanged(int bandIndex, double newValue)
        {
            this.SaveManualPreset();
        }
        #endregion
    }
}