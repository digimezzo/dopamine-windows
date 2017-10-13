using Digimezzo.Utilities.Helpers;
using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.IO;
using Dopamine.Common.Prism;
using Dopamine.Common.Services.Playback;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsAppearanceViewModel : BindableBase
    {
        private IPlaybackService playbackService;
        private bool checkBoxShowSpectrumAnalyzerChecked;
        private bool checkBoxCheckBoxShowWindowBorderChecked;
        private bool checkBoxEnableTransparencyChecked;
        private IEventAggregator eventAggregator;
        private ObservableCollection<NameValue> spectrumStyles;
        private NameValue selectedSpectrumStyle;

        public DelegateCommand<string> OpenColorSchemesDirectoryCommand { get; set; }

        public string ColorSchemesDirectory { get; set; }

        public bool CheckBoxCheckBoxShowWindowBorderChecked
        {
            get { return this.checkBoxCheckBoxShowWindowBorderChecked; }
            set
            {
                SettingsClient.Set<bool>("Appearance", "ShowWindowBorder", value, true);
                SetProperty<bool>(ref this.checkBoxCheckBoxShowWindowBorderChecked, value);
            }
        }

        public bool CheckBoxShowSpectrumAnalyzerChecked
        {
            get { return this.checkBoxShowSpectrumAnalyzerChecked; }
            set
            {
                SettingsClient.Set<bool>("Playback", "ShowSpectrumAnalyzer", value);
                SetProperty<bool>(ref this.checkBoxShowSpectrumAnalyzerChecked, value);
                this.playbackService.IsSpectrumVisible = value;
            }
        }

        public bool CheckBoxEnableTransparencyChecked
        {
            get { return this.checkBoxEnableTransparencyChecked; }
            set
            {
                SettingsClient.Set<bool>("Appearance", "EnableTransparency", value);
                SetProperty<bool>(ref this.checkBoxEnableTransparencyChecked, value);
            }
        }

        public bool IsWindows10 => Constants.IsWindows10;

        public ObservableCollection<NameValue> SpectrumStyles
        {
            get { return this.spectrumStyles; }
            set { SetProperty<ObservableCollection<NameValue>>(ref this.spectrumStyles, value); }
        }

        public NameValue SelectedSpectrumStyle
        {
            get { return this.selectedSpectrumStyle; }
            set
            {
                SetProperty<NameValue>(ref this.selectedSpectrumStyle, value);
                SettingsClient.Set<int>("Playback", "SpectrumStyle", value.Value, true);
            }
        }

        public SettingsAppearanceViewModel(IPlaybackService playbackService, IEventAggregator eventAggregator)
        {
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;

            this.ColorSchemesDirectory = System.IO.Path.Combine(SettingsClient.ApplicationFolder(), ApplicationPaths.ColorSchemesFolder);

            this.OpenColorSchemesDirectoryCommand = new DelegateCommand<string>((colorSchemesDirectory) =>
            {
                try
                {
                    Actions.TryOpenPath(colorSchemesDirectory);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open the ColorSchemes directory. Exception: {0}", ex.Message);
                }
            });

            this.eventAggregator.GetEvent<SelectedSpectrumStyleChanged>().Subscribe((_) => this.SetSelectedSpectrumStyle());

            this.GetCheckBoxesAsync();
            this.GetSpectrumStylesAsync();
        }

        public async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxShowSpectrumAnalyzerChecked = SettingsClient.Get<bool>("Playback", "ShowSpectrumAnalyzer");
                this.checkBoxCheckBoxShowWindowBorderChecked = SettingsClient.Get<bool>("Appearance", "ShowWindowBorder");
                this.checkBoxEnableTransparencyChecked = SettingsClient.Get<bool>("Appearance", "EnableTransparency");
            });
        }

        private async void GetSpectrumStylesAsync()
        {
            var localSpectrumStyles = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localSpectrumStyles.Add(new NameValue { Name = "Flames", Value = 1 });
                localSpectrumStyles.Add(new NameValue { Name = "Lines", Value = 2 });
                localSpectrumStyles.Add(new NameValue { Name = "Bars", Value = 3 });
                localSpectrumStyles.Add(new NameValue { Name = "Stripes", Value = 4 });
            });

            this.SpectrumStyles = localSpectrumStyles;

            this.SetSelectedSpectrumStyle();
        }

        private async void SetSelectedSpectrumStyle()
        {
            NameValue localSelectedSpectrumStyle = null;
            await Task.Run(() => localSelectedSpectrumStyle = this.SpectrumStyles.Where((s) => s.Value == SettingsClient.Get<int>("Playback", "SpectrumStyle")).Select((s) => s).First());
            this.SelectedSpectrumStyle = localSelectedSpectrumStyle;
        }
    }
}
