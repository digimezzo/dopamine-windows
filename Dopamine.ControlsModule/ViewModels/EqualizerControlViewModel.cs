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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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

        private bool canApplyManualPreset;

        private double band0;
        private double band1;
        private double band2;
        private double band3;
        private double band4;
        private double band5;
        private double band6;
        private double band7;
        private double band8;
        private double band9;
        #endregion

        #region Commands
        public DelegateCommand ResetCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        #endregion

        #region Properties
        public double Band0
        {
            get { return this.band0; }
            set
            {
                SetProperty<double>(ref this.band0, Math.Round( value,1));
                OnPropertyChanged(() => this.Band0Label);
                this.ApplyManualPreset();
            }
        }

        public string Band0Label
        {
            get { return this.FormatLabel(this.band0); }
        }

        public double Band1
        {
            get { return this.band1; }
            set
            {
                SetProperty<double>(ref this.band1, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band1Label);
                this.ApplyManualPreset();
            }
        }

        public string Band1Label
        {
            get { return this.FormatLabel(this.band1); }
        }

        public double Band2
        {
            get { return this.band2; }
            set
            {
                SetProperty<double>(ref this.band2, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band2Label);
                this.ApplyManualPreset();
            }
        }

        public string Band2Label
        {
            get { return this.FormatLabel(this.band2); }
        }

        public double Band3
        {
            get { return this.band3; }
            set
            {
                SetProperty<double>(ref this.band3, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band3Label);
                this.ApplyManualPreset();
            }
        }

        public string Band3Label
        {
            get { return this.FormatLabel(this.band3); }
        }

        public double Band4
        {
            get { return this.band4; }
            set
            {
                SetProperty<double>(ref this.band4, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band4Label);
                this.ApplyManualPreset();
            }
        }

        public string Band4Label
        {
            get { return this.FormatLabel(this.band4); }
        }

        public double Band5
        {
            get { return this.band5; }
            set
            {
                SetProperty<double>(ref this.band5, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band5Label);
                this.ApplyManualPreset();
            }
        }

        public string Band5Label
        {
            get { return this.FormatLabel(this.band5); }
        }

        public double Band6
        {
            get { return this.band6; }
            set
            {
                SetProperty<double>(ref this.band6, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band6Label);
                this.ApplyManualPreset();
            }
        }

        public string Band6Label
        {
            get { return this.FormatLabel(this.band6); }
        }

        public double Band7
        {
            get { return this.band7; }
            set
            {
                SetProperty<double>(ref this.band7, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band7Label);
                this.ApplyManualPreset();
            }
        }

        public string Band7Label
        {
            get { return this.FormatLabel(this.band7); }
        }

        public double Band8
        {
            get { return this.band8; }
            set
            {
                SetProperty<double>(ref this.band8, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band8Label);
                this.ApplyManualPreset();
            }
        }

        public string Band8Label
        {
            get { return this.FormatLabel(this.band8); }
        }

        public double Band9
        {
            get { return this.band9; }
            set
            {
                SetProperty<double>(ref this.band9, Math.Round(value, 1));
                OnPropertyChanged(() => this.Band9Label);
                this.ApplyManualPreset();
            }
        }

        public string Band9Label
        {
            get { return this.FormatLabel(this.band9); }
        }

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
                if (value.Name == Defaults.ManualPresetName) value.DisplayName = ResourceUtils.GetStringResource("Language_Manual");
                SetProperty<EqualizerPreset>(ref this.selectedPreset, value);
                this.SetBandValues();
                this.ApplySelectedPreset();
                this.DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsEqualizerEnabled
        {
            get { return this.isEqualizerEnabled; }
            set
            {
                SetProperty<bool>(ref this.isEqualizerEnabled, value);
                this.playbackService.SetIsEqualizerEnabled(value);
                XmlSettingsClient.Instance.Set<bool>("Equalizer", "IsEnabled",value);
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
                this.canApplyManualPreset = false;
                this.Band0 = this.Band1 = this.Band2 = this.Band3 = this.Band4 = this.Band5 = this.Band6 = this.Band7 = this.Band8 = this.Band9 = 0.0;
                this.canApplyManualPreset = true;

                this.ApplyManualPreset();
            });

            this.DeleteCommand = new DelegateCommand(async () => { await this.DeletePresetAsync(); }, () =>
             {
                 if (this.SelectedPreset != null)
                 {
                     return this.SelectedPreset.IsRemovable;
                 }
                 else
                 {
                     return false;
                 }
             });

            this.SaveCommand = new DelegateCommand(async () => { await this.SavePresetToFileAsync(); });

            // Initialize
            this.InitializeAsync();
        }
        #endregion

        #region Private
        private void ApplySelectedPreset()
        {
            XmlSettingsClient.Instance.Set<string>("Equalizer", "SelectedPreset", this.SelectedPreset.Name);
            this.playbackService.ApplyPreset(new EqualizerPreset(this.SelectedPreset.Name, this.SelectedPreset.IsRemovable) { Bands = this.SelectedPreset.Bands });
        }

        private void ApplyManualPreset()
        {
            if (!this.canApplyManualPreset) return;

            EqualizerPreset manualPreset = this.Presets.Select((p) => p).Where((p) => p.Name == Defaults.ManualPresetName).FirstOrDefault();
            manualPreset.Load(this.Band0, this.Band1, this.Band2, this.Band3, this.Band4, this.Band5, this.Band6, this.Band7, this.Band8, this.Band9);

            XmlSettingsClient.Instance.Set<string>("Equalizer", "ManualPreset", manualPreset.ToValueString());

            // Once a slider has moved, revert to the manual preset (also in the settings).
            if (this.SelectedPreset.Name != Defaults.ManualPresetName)
            {
                this.SelectedPreset = manualPreset;
                XmlSettingsClient.Instance.Set<string>("Equalizer", "SelectedPreset", Defaults.ManualPresetName);
            }

            this.playbackService.ApplyPreset(manualPreset);
        }

        private string FormatLabel(double value)
        {
            return value >= 0 ? value.ToString("+0.0") : value.ToString("0.0");
        }

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

            this.SetBandValues();
        }

        private void SetBandValues()
        {
            this.canApplyManualPreset = false;

            this.Band0 = this.SelectedPreset.Bands[0];
            this.Band1 = this.SelectedPreset.Bands[1];
            this.Band2 = this.SelectedPreset.Bands[2];
            this.Band3 = this.SelectedPreset.Bands[3];
            this.Band4 = this.SelectedPreset.Bands[4];
            this.Band5 = this.SelectedPreset.Bands[5];
            this.Band6 = this.SelectedPreset.Bands[6];
            this.Band7 = this.SelectedPreset.Bands[7];
            this.Band8 = this.SelectedPreset.Bands[8];
            this.Band9 = this.SelectedPreset.Bands[9];

            this.canApplyManualPreset = true;
        }

        private async Task SavePresetToFileAsync()
        {
            var showSaveDialog = true;

            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = string.Empty;
            dlg.DefaultExt = FileFormats.DEQ;
            dlg.Filter = string.Concat(ResourceUtils.GetStringResource("Language_Equalizer_Presets"), " (", FileFormats.DEQ, ")|*", FileFormats.DEQ);
            dlg.InitialDirectory = System.IO.Path.Combine(LegacyPaths.AppData(), ProductInformation.ApplicationAssemblyName, ApplicationPaths.EqualizerFolder);

            while (showSaveDialog)
            {
                if ((bool)dlg.ShowDialog())
                {
                    int existingCount = this.presets.Select((p) => p).Where((p) => p.Name.ToLower() == System.IO.Path.GetFileNameWithoutExtension(dlg.FileName).ToLower() & !p.IsRemovable).Count();

                    if (existingCount > 0)
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
                            await Task.Run(() => {
                                System.IO.File.WriteAllLines(dlg.FileName, this.SelectedPreset.ToValueString().Split(';'));
                            });
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

        private async Task DeletePresetAsync()
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetStringResource("Language_Delete_Preset"),
                ResourceUtils.GetStringResource("Language_Delete_Preset_Confirmation").Replace("%preset%", this.SelectedPreset.Name),
                ResourceUtils.GetStringResource("Language_Yes"),
                ResourceUtils.GetStringResource("Language_No")))
            {
                try
                {
                    await Task.Run(() => {
                        string presetPath = System.IO.Path.Combine(LegacyPaths.AppData(), ProductInformation.ApplicationAssemblyName, ApplicationPaths.EqualizerFolder, this.SelectedPreset.Name + FileFormats.DEQ);
                        System.IO.File.Delete(presetPath);
                    });

                    this.ApplyManualPreset();
                    this.InitializeAsync();
                }
                catch (Exception ex)
                {
                    LogClient.Instance.Logger.Error("An error occured while deleting preset '{0}'. Exception: {1}", this.SelectedPreset, ex.Message);

                    this.dialogService.ShowNotification(
                                        0xe711,
                                        16,
                                        ResourceUtils.GetStringResource("Language_Error"),
                                        ResourceUtils.GetStringResource("Language_Error_While_Deleting_Preset"),
                                        ResourceUtils.GetStringResource("Language_Ok"),
                                        true,
                                        ResourceUtils.GetStringResource("Language_Log_File"));
                }
            }
        }
        #endregion
    }
}