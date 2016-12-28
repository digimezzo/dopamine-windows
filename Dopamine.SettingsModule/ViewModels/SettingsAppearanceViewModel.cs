using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.IO;
using Dopamine.Common.Prism;
using Prism.Events;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows;

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
        #endregion

        #region Construction
        public SettingsAppearanceViewModel(IPlaybackService playbackService, IEventAggregator eventAggregator)
        {
            this.playbackService = playbackService;
            this.eventAggregator = eventAggregator;

            this.ColorSchemesDirectory = System.IO.Path.Combine(SettingsClient.ApplicationFolder(), ApplicationPaths.ColorSchemesFolder);

            this.GetCheckBoxesAsync();
        }
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
        #endregion
    }

}
