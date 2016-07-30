using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Taskbar;
using Dopamine.Core.Base;
using Dopamine.Core.Helpers;
using Dopamine.Core.Settings;
using Dopamine.Core.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsPlaybackViewModel : BindableBase
    {
        #region Variables
        private ObservableCollection<NameValue> mLatencies;
        private NameValue mSelectedLatency;
        private IPlaybackService mPlaybackService;
        private ITaskbarService mTaskbarService;
        private INotificationService mNotificationService;
        private bool mCheckBoxWasapiExclusiveModeChecked;
        private bool mCheckBoxShowNotificationChecked;
        private bool mCheckBoxShowNotificationControlsChecked;
        private bool mCheckBoxShowProgressInTaskbarChecked;
        private bool mCheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked;
        private ObservableCollection<NameValue> mNotificationPositions;
        private NameValue mSelectedNotificationPosition;
        private ObservableCollection<int> mNotificationSeconds;
        private int mSelectedNotificationSecond;
        #endregion

        #region Commands
        public DelegateCommand ShowTestNotificationCommand { get; set; }
        #endregion

        #region Properties
        public ObservableCollection<NameValue> Latencies
        {
            get { return mLatencies; }
            set { SetProperty<ObservableCollection<NameValue>>(ref mLatencies, value); }
        }

        public NameValue SelectedLatency
        {
            get { return mSelectedLatency; }
            set
            {
                XmlSettingsClient.Instance.Set<int>("Playback", "AudioLatency", value.Value);
                SetProperty<NameValue>(ref mSelectedLatency, value);

                if (mPlaybackService != null)
                {
                    mPlaybackService.Latency = value.Value;
                }
            }
        }

        public bool CheckBoxWasapiExclusiveModeChecked
        {
            get { return this.mCheckBoxWasapiExclusiveModeChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Playback", "WasapiExclusiveMode", value);
                SetProperty<bool>(ref mCheckBoxWasapiExclusiveModeChecked, value);

                if (mPlaybackService != null)
                {
                    mPlaybackService.ExclusiveMode = value;
                }
            }
        }

        public bool CheckBoxShowNotificationChecked
        {
            get { return mCheckBoxShowNotificationChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "ShowNotification", value);
                SetProperty<bool>(ref mCheckBoxShowNotificationChecked, value);
            }
        }

        public bool CheckBoxShowNotificationControlsChecked
        {
            get { return mCheckBoxShowNotificationControlsChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "ShowNotificationControls", value);
                SetProperty<bool>(ref mCheckBoxShowNotificationControlsChecked, value);
            }
        }

        public bool CheckBoxShowProgressInTaskbarChecked
        {
            get { return mCheckBoxShowProgressInTaskbarChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Playback", "ShowProgressInTaskbar", value);
                SetProperty<bool>(ref this.mCheckBoxShowProgressInTaskbarChecked, value);

                if (mTaskbarService != null && mPlaybackService != null)
                {
                    mTaskbarService.SetTaskbarProgressState(value, mPlaybackService.IsPlaying);
                }
            }
        }

        public bool CheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked
        {
            get { return mCheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible", value);
                SetProperty<bool>(ref mCheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked, value);
            }
        }

        public ObservableCollection<NameValue> NotificationPositions
        {
            get { return mNotificationPositions; }
            set { SetProperty<ObservableCollection<NameValue>>(ref mNotificationPositions, value); }
        }

        public NameValue SelectedNotificationPosition
        {
            get { return mSelectedNotificationPosition; }
            set
            {
                XmlSettingsClient.Instance.Set<int>("Behaviour", "NotificationPosition", value.Value);
                SetProperty<NameValue>(ref mSelectedNotificationPosition, value);
            }
        }

        public ObservableCollection<int> NotificationSeconds
        {
            get { return mNotificationSeconds; }
            set { SetProperty<ObservableCollection<int>>(ref mNotificationSeconds, value); }
        }

        public int SelectedNotificationSecond
        {
            get { return mSelectedNotificationSecond; }
            set
            {
                XmlSettingsClient.Instance.Set<int>("Behaviour", "NotificationAutoCloseSeconds", value);
                SetProperty<int>(ref mSelectedNotificationSecond, value);
            }
        }
        #endregion

        #region Construction
        public SettingsPlaybackViewModel(IPlaybackService playbackService, ITaskbarService taskbarService, INotificationService notificationService)
        {
            mPlaybackService = playbackService;
            mTaskbarService = taskbarService;
            mNotificationService = notificationService;

            ShowTestNotificationCommand = new DelegateCommand(() => mNotificationService.ShowNotificationAsync());

            this.GetCheckBoxesAsync();
            this.GetNotificationPositionsAsync();
            this.GetNotificationSecondsAsync();
            this.GetLatenciesAsync();
        }
        #endregion

        #region Private
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

            await Task.Run(() => localSelectedLatency = Latencies.Where((pa) => pa.Value == XmlSettingsClient.Instance.Get<int>("Playback", "AudioLatency")).Select((pa) => pa).First());

            this.SelectedLatency = localSelectedLatency;
        }

        private async void GetNotificationPositionsAsync()
        {
            var localNotificationPositions = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetStringResource("Language_Bottom_Left"), Value = (int)NotificationPosition.BottomLeft });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetStringResource("Language_Top_Left"), Value = (int)NotificationPosition.TopLeft });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetStringResource("Language_Top_Right"), Value = (int)NotificationPosition.TopRight });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetStringResource("Language_Bottom_Right"), Value = (int)NotificationPosition.BottomRight });
            });

            this.NotificationPositions = localNotificationPositions;

            NameValue localSelectedNotificationPosition = null;

            await Task.Run(() => localSelectedNotificationPosition = NotificationPositions.Where((np) => np.Value == XmlSettingsClient.Instance.Get<int>("Behaviour", "NotificationPosition")).Select((np) => np).First());

            this.SelectedNotificationPosition = localSelectedNotificationPosition;
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

            await Task.Run(() => localSelectedNotificationSecond = NotificationSeconds.Where((ns) => ns == XmlSettingsClient.Instance.Get<int>("Behaviour", "NotificationAutoCloseSeconds")).Select((ns) => ns).First());

            this.SelectedNotificationSecond = localSelectedNotificationSecond;
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.CheckBoxWasapiExclusiveModeChecked = XmlSettingsClient.Instance.Get<bool>("Playback", "WasapiExclusiveMode");
                this.CheckBoxShowNotificationChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotification");
                this.CheckBoxShowNotificationControlsChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationControls");
                this.CheckBoxShowProgressInTaskbarChecked = XmlSettingsClient.Instance.Get<bool>("Playback", "ShowProgressInTaskbar");
                this.CheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible");

            });
        }
        #endregion
    }
}
