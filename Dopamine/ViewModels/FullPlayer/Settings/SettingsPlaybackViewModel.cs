using Digimezzo.Foundation.Core.Helpers;
using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Audio;
using Dopamine.Core.Base;
using Dopamine.Services.Dialog;
using Dopamine.Services.ExternalControl;
using Dopamine.Services.I18n;
using Dopamine.Services.Notification;
using Dopamine.Services.Playback;
using Dopamine.Services.Taskbar;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dopamine.Core.Helpers;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsPlaybackViewModel : BindableBase
    {
        private ObservableCollection<NameValue> latencies;
        private NameValue selectedLatency;
        private IPlaybackService playbackService;
        private ITaskbarService taskbarService;
        private INotificationService notificationService;
        private IDialogService dialogService;
        private IExternalControlService externalControlService;
        private II18nService i18nService;
        private bool checkBoxUseAllAvailableChannelsChecked;
        private bool checkBoxWasapiExclusiveModeChecked;
        private bool checkBoxShowNotificationWhenPlayingChecked;
        private bool checkBoxShowNotificationWhenPausingChecked;
        private bool checkBoxShowNotificationWhenResumingChecked;
        private bool checkBoxShowNotificationControlsChecked;
        private bool checkBoxShowProgressInTaskbarChecked;
        private bool checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked;
        private bool checkBoxEnableExternalControlChecked;
        private bool checkBoxEnableSystemNotificationChecked;
        private bool checkBoxLoopWhenShuffleChecked;
        private bool checkBoxShowSpectrumAnalyzerChecked;
        private bool checkBoxPreventSleepWhilePlaying;
        private ObservableCollection<NameValue> spectrumStyles;
        private NameValue selectedSpectrumStyle;
        private ObservableCollection<NameValue> notificationPositions;
        private NameValue selectedNotificationPosition;
        private ObservableCollection<int> notificationSeconds;
        private int selectedNotificationSecond;
        private ObservableCollection<AudioDevice> audioDevices;
        private AudioDevice selectedAudioDevice;

        public DelegateCommand ShowTestNotificationCommand { get; set; }

        public DelegateCommand LoadedCommand { get; set; }

        public bool IsNotificationEnabled => (this.CheckBoxShowNotificationWhenPlayingChecked || this.CheckBoxShowNotificationWhenPausingChecked || this.CheckBoxShowNotificationWhenResumingChecked) && !this.CheckBoxEnableSystemNotificationChecked;

        public bool SupportsWindowsMediaFoundation => MediaFoundationHelper.HasMediaFoundationSupport();

        public bool IsWindows10 => Constants.IsWindows10;

        public bool CheckBoxShowSpectrumAnalyzerChecked
        {
            get { return this.checkBoxShowSpectrumAnalyzerChecked; }
            set
            {
                SettingsClient.Set<bool>("Playback", "ShowSpectrumAnalyzer", value, true);
                SetProperty<bool>(ref this.checkBoxShowSpectrumAnalyzerChecked, value);
            }
        }

        public ObservableCollection<NameValue> Latencies
        {
            get => this.latencies;
            set => SetProperty<ObservableCollection<NameValue>>(ref this.latencies, value);
        }

        public NameValue SelectedLatency
        {
            get => this.selectedLatency;
            set
            {
                if (value != null)
                {
                    SettingsClient.Set<int>("Playback", "AudioLatency", value.Value);
                }

                SetProperty<NameValue>(ref this.selectedLatency, value);

                if (this.playbackService != null)
                {
                    this.playbackService.Latency = value.Value;
                }
            }
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
                if (value != null)
                {
                    SettingsClient.Set<int>("Playback", "SpectrumStyle", value.Value, true);
                }

                SetProperty<NameValue>(ref this.selectedSpectrumStyle, value);
            }
        }

        public bool CheckBoxUseAllAvailableChannelsChecked
        {
            get { return this.checkBoxUseAllAvailableChannelsChecked; }
            set
            {
                SetProperty<bool>(ref this.checkBoxUseAllAvailableChannelsChecked, value);
                SettingsClient.Set<bool>("Playback", "WasapiUseAllAvailableChannels", value);
            }
        }

        public bool CheckBoxWasapiExclusiveModeChecked
        {
            get { return this.checkBoxWasapiExclusiveModeChecked; }
            set
            {
                if (value)
                {
                    this.ConfirmEnableExclusiveMode();
                }
                else
                {
                    this.ApplyExclusiveMode(false);
                }
            }
        }

        public bool CheckBoxShowNotificationWhenPlayingChecked
        {
            get => this.checkBoxShowNotificationWhenPlayingChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationWhenPlayingChecked, value);
                this.notificationService.ShowNotificationWhenPlaying = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public bool CheckBoxShowNotificationWhenPausingChecked
        {
            get => this.checkBoxShowNotificationWhenPausingChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationWhenPausingChecked, value);
                this.notificationService.ShowNotificationWhenPausing = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public bool CheckBoxShowNotificationWhenResumingChecked
        {
            get => this.checkBoxShowNotificationWhenResumingChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationWhenResumingChecked, value);
                this.notificationService.ShowNotificationWhenResuming = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public bool CheckBoxShowNotificationControlsChecked
        {
            get => this.checkBoxShowNotificationControlsChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationControlsChecked, value);
                this.notificationService.ShowNotificationControls = value;
            }
        }

        public bool CheckBoxShowProgressInTaskbarChecked
        {
            get => this.checkBoxShowProgressInTaskbarChecked;
            set
            {
                SettingsClient.Set<bool>("Playback", "ShowProgressInTaskbar", value);
                SetProperty<bool>(ref this.checkBoxShowProgressInTaskbarChecked, value);

                if (this.taskbarService != null && this.playbackService != null)
                {
                    this.taskbarService.SetShowProgressInTaskbar(value);
                }
            }
        }

        public bool CheckBoxLoopWhenShuffleChecked
        {
            get => this.checkBoxLoopWhenShuffleChecked;
            set
            {
                SettingsClient.Set<bool>("Playback", "LoopWhenShuffle", value);
                SetProperty<bool>(ref this.checkBoxLoopWhenShuffleChecked, value);
            }
        }

        public bool CheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked
        {
            get => this.checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked;
            set
            {
                SettingsClient.Set<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible", value);
                SetProperty<bool>(ref this.checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked, value);
            }
        }

        public ObservableCollection<NameValue> NotificationPositions
        {
            get => this.notificationPositions;
            set => SetProperty<ObservableCollection<NameValue>>(ref this.notificationPositions, value);
        }

        public NameValue SelectedNotificationPosition
        {
            get => this.selectedNotificationPosition;
            set
            {
                if (value != null)
                {
                    SettingsClient.Set<int>("Behaviour", "NotificationPosition", value.Value);
                }

                SetProperty<NameValue>(ref this.selectedNotificationPosition, value);
            }
        }

        public ObservableCollection<int> NotificationSeconds
        {
            get => this.notificationSeconds;
            set => SetProperty<ObservableCollection<int>>(ref this.notificationSeconds, value);
        }

        public int SelectedNotificationSecond
        {
            get => this.selectedNotificationSecond;
            set
            {
                SettingsClient.Set<int>("Behaviour", "NotificationAutoCloseSeconds", value);
                SetProperty<int>(ref this.selectedNotificationSecond, value);
            }
        }

        public ObservableCollection<AudioDevice> AudioDevices
        {
            get => this.audioDevices;
            set => SetProperty<ObservableCollection<AudioDevice>>(ref this.audioDevices, value);
        }

        public AudioDevice SelectedAudioDevice
        {
            get => this.selectedAudioDevice;
            set
            {
                SetProperty<AudioDevice>(ref this.selectedAudioDevice, value);

                // Due to two-way binding, this can be null when the list is being filled.
                if (value != null)
                {
                    SettingsClient.Set<string>("Playback", "AudioDevice", value.DeviceId == null ? string.Empty : value.DeviceId);

                    this.playbackService.SwitchAudioDeviceAsync(value);
                }
            }
        }

        public bool CheckBoxEnableExternalControlChecked
        {
            get => this.checkBoxEnableExternalControlChecked;
            set
            {
                SettingsClient.Set("Playback", "EnableExternalControl", value);
                SetProperty(ref this.checkBoxEnableExternalControlChecked, value);
                if (value == true)
                    this.externalControlService.Start();
                else
                    this.externalControlService.Stop();
            }
        }

        public bool CheckBoxEnableSystemNotificationChecked
        {
            get => this.checkBoxEnableSystemNotificationChecked;
            set
            {
                SetProperty(ref this.checkBoxEnableSystemNotificationChecked, value);
                this.notificationService.SystemNotificationIsEnabled = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public bool CheckBoxPreventSleepWhilePlaying
        {
            get => this.checkBoxPreventSleepWhilePlaying;
            set
            {
                SettingsClient.Set<bool>("Playback", "PreventSleepWhilePlaying", value, true);
                SetProperty<bool>(ref this.checkBoxPreventSleepWhilePlaying, value);
            }
        }

        public SettingsPlaybackViewModel(IPlaybackService playbackService, ITaskbarService taskbarService, INotificationService notificationService, IDialogService dialogService, IExternalControlService externalControlService, II18nService i18nService)
        {
            this.playbackService = playbackService;
            this.taskbarService = taskbarService;
            this.notificationService = notificationService;
            this.dialogService = dialogService;
            this.externalControlService = externalControlService;
            this.i18nService = i18nService;

            this.ShowTestNotificationCommand = new DelegateCommand(() => this.notificationService.ShowNotificationAsync());
            this.LoadedCommand = new DelegateCommand(() => this.GetAudioDevicesAsync());

            this.i18nService.LanguageChanged += (_, __) =>
            {
                this.GetNotificationPositionsAsync();
                this.GetLatenciesAsync();
                this.GetAudioDevicesAsync();
                this.GetSpectrumStylesAsync();
            };

            this.GetCheckBoxesAsync();
            this.GetNotificationPositionsAsync();
            this.GetNotificationSecondsAsync();
            this.GetLatenciesAsync();
            this.GetAudioDevicesAsync();
            this.GetSpectrumStylesAsync();
        }

        private async void GetAudioDevicesAsync()
        {
            IList<AudioDevice> audioDevices = await this.playbackService.GetAllAudioDevicesAsync();

            this.AudioDevices = new ObservableCollection<AudioDevice>();

            this.AudioDevices.AddRange(audioDevices);

            AudioDevice savedAudioDevice = await this.playbackService.GetSavedAudioDeviceAsync();

            this.selectedAudioDevice = null;
            RaisePropertyChanged(nameof(this.SelectedAudioDevice));
            this.selectedAudioDevice = savedAudioDevice == null ? this.AudioDevices.First() : savedAudioDevice;
            RaisePropertyChanged(nameof(this.SelectedAudioDevice));
        }

        private async void GetLatenciesAsync()
        {
            var localLatencies = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                // Increment by 50
                for (int index = 50; index <= 500; index += 50)
                {
                    if (index == 200)
                    {
                        localLatencies.Add(new NameValue
                        {
                            Name = index + " ms (" + Application.Current.FindResource("Language_Default").ToString().ToLower() + ")",
                            Value = index
                        });
                    }
                    else
                    {
                        localLatencies.Add(new NameValue
                        {
                            Name = index + " ms",
                            Value = index
                        });
                    }
                }
            });

            this.Latencies = localLatencies;

            NameValue localSelectedLatency = null;
            await Task.Run(() => localSelectedLatency = this.Latencies.Where((pa) => pa.Value == SettingsClient.Get<int>("Playback", "AudioLatency")).Select((pa) => pa).First());

            this.selectedLatency = null;
            RaisePropertyChanged(nameof(this.SelectedLatency));
            this.selectedLatency = localSelectedLatency;
            RaisePropertyChanged(nameof(this.SelectedLatency));
        }

        private async void GetNotificationPositionsAsync()
        {
            var localNotificationPositions = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Bottom_Left"), Value = (int)NotificationPosition.BottomLeft });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Top_Left"), Value = (int)NotificationPosition.TopLeft });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Top_Right"), Value = (int)NotificationPosition.TopRight });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Bottom_Right"), Value = (int)NotificationPosition.BottomRight });
            });

            this.NotificationPositions = localNotificationPositions;

            NameValue localSelectedNotificationPosition = null;
            await Task.Run(() => localSelectedNotificationPosition = NotificationPositions.Where((np) => np.Value == SettingsClient.Get<int>("Behaviour", "NotificationPosition")).Select((np) => np).First());

            this.selectedNotificationPosition = null;
            RaisePropertyChanged(nameof(this.SelectedNotificationPosition));
            this.selectedNotificationPosition = localSelectedNotificationPosition;
            RaisePropertyChanged(nameof(this.SelectedNotificationPosition));
        }

        private async void GetNotificationSecondsAsync()
        {
            var localNotificationSeconds = new ObservableCollection<int>();

            await Task.Run(() =>
            {
                for (int index = 1; index <= 5; index++)
                {
                    localNotificationSeconds.Add(index);
                }

            });

            this.NotificationSeconds = localNotificationSeconds;

            int localSelectedNotificationSecond = 0;
            await Task.Run(() => localSelectedNotificationSecond = NotificationSeconds.Where((ns) => ns == SettingsClient.Get<int>("Behaviour", "NotificationAutoCloseSeconds")).Select((ns) => ns).First());

            this.SelectedNotificationSecond = localSelectedNotificationSecond;
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxShowSpectrumAnalyzerChecked = SettingsClient.Get<bool>("Playback", "ShowSpectrumAnalyzer");
                this.checkBoxUseAllAvailableChannelsChecked = SettingsClient.Get<bool>("Playback", "WasapiUseAllAvailableChannels");
                this.checkBoxWasapiExclusiveModeChecked = SettingsClient.Get<bool>("Playback", "WasapiExclusiveMode");
                this.checkBoxShowNotificationWhenPlayingChecked = this.notificationService.ShowNotificationWhenPlaying;
                this.checkBoxShowNotificationWhenPausingChecked = this.notificationService.ShowNotificationWhenPausing;
                this.checkBoxShowNotificationWhenResumingChecked = this.notificationService.ShowNotificationWhenResuming;
                this.checkBoxShowNotificationControlsChecked = this.notificationService.ShowNotificationControls;
                this.checkBoxShowProgressInTaskbarChecked = SettingsClient.Get<bool>("Playback", "ShowProgressInTaskbar");
                this.checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked = SettingsClient.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible");
                this.checkBoxEnableExternalControlChecked = SettingsClient.Get<bool>("Playback", "EnableExternalControl");
                this.checkBoxLoopWhenShuffleChecked = SettingsClient.Get<bool>("Playback", "LoopWhenShuffle");
                this.checkBoxPreventSleepWhilePlaying = SettingsClient.Get<bool>("Playback", "PreventSleepWhilePlaying");
                this.checkBoxEnableSystemNotificationChecked = this.notificationService.SystemNotificationIsEnabled;
            });
        }

        private async void GetSpectrumStylesAsync()
        {
            var localSpectrumStyles = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localSpectrumStyles.Add(new NameValue { Name = ResourceUtils.GetString("Language_Spectrum_Flames"), Value = 1 });
                localSpectrumStyles.Add(new NameValue { Name = ResourceUtils.GetString("Language_Spectrum_Lines"), Value = 2 });
                localSpectrumStyles.Add(new NameValue { Name = ResourceUtils.GetString("Language_Spectrum_Bars"), Value = 3 });
            });

            this.SpectrumStyles = localSpectrumStyles;

            NameValue localSelectedSpectrumStyle = null;
            await Task.Run(() =>
            {
                localSelectedSpectrumStyle = this.SpectrumStyles.Where((s) => s.Value == SettingsClient.Get<int>("Playback", "SpectrumStyle")).Select((s) => s).FirstOrDefault();

                if (localSelectedSpectrumStyle == null)
                {
                    localSelectedSpectrumStyle = this.SpectrumStyles.First();
                }
            });

            this.selectedSpectrumStyle = null;
            RaisePropertyChanged(nameof(this.SelectedSpectrumStyle));
            this.selectedSpectrumStyle = localSelectedSpectrumStyle;
            RaisePropertyChanged(nameof(this.SelectedSpectrumStyle));
        }

        private void ConfirmEnableExclusiveMode()
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetString("Language_Exclusive_Mode"),
                ResourceUtils.GetString("Language_Exclusive_Mode_Confirmation"),
                ResourceUtils.GetString("Language_Yes"),
                ResourceUtils.GetString("Language_No")))
            {
                ApplyExclusiveMode(true);
            }
        }

        private void ApplyExclusiveMode(bool isEnabled)
        {
            SettingsClient.Set<bool>("Playback", "WasapiExclusiveMode", isEnabled);
            SetProperty<bool>(ref this.checkBoxWasapiExclusiveModeChecked, isEnabled);
            if (this.playbackService != null) this.playbackService.ExclusiveMode = isEnabled;
        }
    }
}
