using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.IO;
using Dopamine.Common.Prism;
using Prism.Events;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using Dopamine.Common.Enums;
using Digimezzo.Utilities.Helpers;
using System.Linq;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsAppearanceViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playbackService;
        private bool checkBoxShowSpectrumAnalyzerChecked;
        private bool checkBoxCheckBoxShowWindowBorderChecked;
        private bool checkBoxEnableTransparencyChecked;
        private IEventAggregator eventAggregator;
        private ObservableCollection<NameValue> spectrumStyles;
        private NameValue selectedSpectrumStyle;
        #endregion

        #region Properties
        public string ColorSchemesDirectory { get; set; }

        public bool CheckBoxCheckBoxShowWindowBorderChecked
        {
            get { return this.checkBoxCheckBoxShowWindowBorderChecked; }
            set
            {
                SettingsClient.Set<bool>("Appearance", "ShowWindowBorder", value);
                SetProperty<bool>(ref this.checkBoxCheckBoxShowWindowBorderChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingShowWindowBorderChanged>().Publish(value));
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

        public bool IsWindows10
        {
            get { return EnvironmentUtils.IsWindows10(); }
        }

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
                SettingsClient.Set<int>("Playback", "SpectrumStyle", value.Value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingSpectrumStyleChanged>().Publish((SpectrumStyle)value.Value));
            }
        }
        #endregion

        #region Construction
        public SettingsAppearanceViewModel(IPlaybackService playbackService, IEventAggregator eventAggregator)
        {
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;

            this.ColorSchemesDirectory = System.IO.Path.Combine(SettingsClient.ApplicationFolder(), ApplicationPaths.ColorSchemesFolder);

            this.GetCheckBoxesAsync();
            this.GetSpectrumStylesAsync();
        }
        #endregion

        #region Private
        public async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.CheckBoxShowSpectrumAnalyzerChecked = SettingsClient.Get<bool>("Playback", "ShowSpectrumAnalyzer");
                this.CheckBoxCheckBoxShowWindowBorderChecked = SettingsClient.Get<bool>("Appearance", "ShowWindowBorder");
                this.CheckBoxEnableTransparencyChecked = SettingsClient.Get<bool>("Appearance", "EnableTransparency");
            });
        }

        private async void GetSpectrumStylesAsync()
        {
            var localSpectrumStyles = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localSpectrumStyles.Add(new NameValue { Name = "Dopamine", Value = 1 });
                localSpectrumStyles.Add(new NameValue { Name = "Zune", Value = 2 });
            });

            this.SpectrumStyles = localSpectrumStyles;

            NameValue localSelectedSpectrumStyle = null;
            await Task.Run(() => localSelectedSpectrumStyle = this.SpectrumStyles.Where((s) => s.Value == SettingsClient.Get<int>("Playback", "SpectrumStyle")).Select((s) => s).First());
            this.SelectedSpectrumStyle = localSelectedSpectrumStyle;
        }
        #endregion
    }

}
