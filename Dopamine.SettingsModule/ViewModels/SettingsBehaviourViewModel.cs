using Digimezzo.Utilities.Helpers;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Prism;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsBehaviourViewModel : BindableBase
    {
        #region Variables
        private IEventAggregator eventAggregator;
        private bool checkBoxShowTrayIconChecked;
        private bool checkBoxMinimizeToTrayChecked;
        private bool checkBoxFollowTrackChecked;
        private bool checkBoxCloseToTrayChecked;
        private bool checkBoxEnableRatingChecked;
        private bool checkBoxEnableLoveChecked;
        private bool checkBoxSaveRatingInAudioFilesChecked;
        private bool checkBoxShowRemoveFromDiskChecked;
        private ObservableCollection<NameValue> scrollVolumePercentages;
        private NameValue selectedScrollVolumePercentage;
        #endregion

        #region Properties
        public bool CheckBoxShowTrayIconChecked
        {
            get { return this.checkBoxShowTrayIconChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "ShowTrayIcon", value);
                SetProperty<bool>(ref this.checkBoxShowTrayIconChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingShowTrayIconChanged>().Publish(value));
            }
        }

        public bool CheckBoxMinimizeToTrayChecked
        {
            get { return this.checkBoxMinimizeToTrayChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "MinimizeToTray", value);
                SetProperty<bool>(ref this.checkBoxMinimizeToTrayChecked, value);
            }
        }

        public bool CheckBoxCloseToTrayChecked
        {
            get { return this.checkBoxCloseToTrayChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "CloseToTray", value);
                SetProperty<bool>(ref this.checkBoxCloseToTrayChecked, value);
            }
        }

        public bool CheckBoxFollowTrackChecked
        {
            get { return this.checkBoxFollowTrackChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "FollowTrack", value);
                SetProperty<bool>(ref this.checkBoxFollowTrackChecked, value);
            }
        }

        public bool CheckBoxEnableRatingChecked
        {
            get { return this.checkBoxEnableRatingChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "EnableRating", value);
                SetProperty<bool>(ref this.checkBoxEnableRatingChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Publish(value));
            }
        }

        public bool CheckBoxEnableLoveChecked
        {
            get { return this.checkBoxEnableLoveChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "EnableLove", value);
                SetProperty<bool>(ref this.checkBoxEnableLoveChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Publish(value));
            }
        }

        public bool CheckBoxSaveRatingInAudioFilesChecked
        {
            get { return this.checkBoxSaveRatingInAudioFilesChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "SaveRatingToAudioFiles", value);
                SetProperty<bool>(ref this.checkBoxSaveRatingInAudioFilesChecked, value);
            }
        }

        public bool CheckBoxShowRemoveFromDiskChecked
        {
            get { return this.checkBoxShowRemoveFromDiskChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "ShowRemoveFromDisk", value);
                SetProperty<bool>(ref this.checkBoxShowRemoveFromDiskChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingShowRemoveFromDiskChanged>().Publish(value));
            }
        }

    public ObservableCollection<NameValue> ScrollVolumePercentages
        {
            get { return this.scrollVolumePercentages; }
            set { SetProperty<ObservableCollection<NameValue>>(ref this.scrollVolumePercentages, value); }
        }

        public NameValue SelectedScrollVolumePercentage
        {
            get { return this.selectedScrollVolumePercentage; }
            set
            {
                SettingsClient.Set<int>("Behaviour", "ScrollVolumePercentage", value.Value);
                SetProperty<NameValue>(ref this.selectedScrollVolumePercentage, value);
            }
        }
        #endregion

        #region Construction
        public SettingsBehaviourViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.GetCheckBoxesAsync();
            this.GetScrollVolumePercentagesAsync();
        }
        #endregion

        #region Private
        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.CheckBoxShowTrayIconChecked = SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon");
                this.CheckBoxMinimizeToTrayChecked = SettingsClient.Get<bool>("Behaviour", "MinimizeToTray");
                this.CheckBoxCloseToTrayChecked = SettingsClient.Get<bool>("Behaviour", "CloseToTray");
                this.CheckBoxFollowTrackChecked = SettingsClient.Get<bool>("Behaviour", "FollowTrack");
                this.CheckBoxEnableRatingChecked = SettingsClient.Get<bool>("Behaviour", "EnableRating");
                this.CheckBoxEnableLoveChecked = SettingsClient.Get<bool>("Behaviour", "EnableLove");
                this.CheckBoxShowRemoveFromDiskChecked = SettingsClient.Get<bool>("Behaviour", "ShowRemoveFromDisk");
                this.CheckBoxSaveRatingInAudioFilesChecked = SettingsClient.Get<bool>("Behaviour", "SaveRatingToAudioFiles");
            });
        }

        private async void GetScrollVolumePercentagesAsync()
        {
            var localScrollVolumePercentages = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localScrollVolumePercentages.Add(new NameValue { Name = "1 %", Value = 1 });
                localScrollVolumePercentages.Add(new NameValue { Name = "2 %", Value = 2 });
                localScrollVolumePercentages.Add(new NameValue { Name = "5 %", Value = 5 });
                localScrollVolumePercentages.Add(new NameValue { Name = "10 %", Value = 10 });
                localScrollVolumePercentages.Add(new NameValue { Name = "15 %", Value = 15 });
                localScrollVolumePercentages.Add(new NameValue { Name = "20 %", Value = 20 });
            });

            this.ScrollVolumePercentages = localScrollVolumePercentages;

            NameValue localSelectedScrollVolumePercentage = null;
            await Task.Run(() => localSelectedScrollVolumePercentage = this.ScrollVolumePercentages.Where((svp) => svp.Value == SettingsClient.Get<int>("Behaviour", "ScrollVolumePercentage")).Select((svp) => svp).First());
            this.SelectedScrollVolumePercentage = localSelectedScrollVolumePercentage;
        }
        #endregion
    }
}
