using Digimezzo.Foundation.Core.Helpers;
using Digimezzo.Foundation.Core.Settings;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsBehaviourViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;
        private bool checkBoxUseAppCommandMediaKeysChecked;
        private bool checkBoxShowTrayIconChecked;
        private bool checkBoxMinimizeToTrayChecked;
        private bool checkBoxFollowTrackChecked;
        private bool checkBoxCloseToTrayChecked;
        private bool checkBoxEnableRatingChecked;
        private bool checkBoxEnableLoveChecked;
        private bool checkBoxSaveRatingInAudioFilesChecked;
        private bool checkBoxShowRemoveFromDiskChecked;
        private bool checkBoxUseLogarithmicVolumeScaleChecked;
        private ObservableCollection<NameValue> scrollVolumePercentages;
        private NameValue selectedScrollVolumePercentage;

        public bool CheckBoxUseAppCommandMediaKeysChecked
        {
            get { return this.checkBoxUseAppCommandMediaKeysChecked; }
            set
            {
                SettingsClient.Set<bool>("MediaKeys", "UseAppCommandMediaKeys", value, true);
                SetProperty<bool>(ref this.checkBoxUseAppCommandMediaKeysChecked, value);
            }
        }


        public bool CheckBoxShowTrayIconChecked
        {
            get { return this.checkBoxShowTrayIconChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "ShowTrayIcon", value, true);
                SetProperty<bool>(ref this.checkBoxShowTrayIconChecked, value);
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
                SettingsClient.Set<bool>("Behaviour", "EnableRating", value, true);
                SetProperty<bool>(ref this.checkBoxEnableRatingChecked, value);
            }
        }

        public bool CheckBoxEnableLoveChecked
        {
            get { return this.checkBoxEnableLoveChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "EnableLove", value, true);
                SetProperty<bool>(ref this.checkBoxEnableLoveChecked, value);
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
                SettingsClient.Set<bool>("Behaviour", "ShowRemoveFromDisk", value, true);
                SetProperty<bool>(ref this.checkBoxShowRemoveFromDiskChecked, value);
            }
        }

        public bool CheckBoxUseLogarithmicVolumeScaleChecked
        {
            get { return this.checkBoxUseLogarithmicVolumeScaleChecked; }
            set
            {
                SettingsClient.Set<bool>("Behaviour", "UseLogarithmicVolumeScale", value, true);
                SetProperty<bool>(ref this.checkBoxUseLogarithmicVolumeScaleChecked, value);
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
                if(value != null)
                {
                    SettingsClient.Set<int>("Behaviour", "ScrollVolumePercentage", value.Value);
                }

                SetProperty<NameValue>(ref this.selectedScrollVolumePercentage, value);
            }
        }

        public SettingsBehaviourViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.GetCheckBoxesAsync();
            this.GetScrollVolumePercentagesAsync();
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxUseAppCommandMediaKeysChecked = SettingsClient.Get<bool>("MediaKeys", "UseAppCommandMediaKeys");
                this.checkBoxShowTrayIconChecked = SettingsClient.Get<bool>("Behaviour", "ShowTrayIcon");
                this.checkBoxMinimizeToTrayChecked = SettingsClient.Get<bool>("Behaviour", "MinimizeToTray");
                this.checkBoxCloseToTrayChecked = SettingsClient.Get<bool>("Behaviour", "CloseToTray");
                this.checkBoxFollowTrackChecked = SettingsClient.Get<bool>("Behaviour", "FollowTrack");
                this.checkBoxEnableRatingChecked = SettingsClient.Get<bool>("Behaviour", "EnableRating");
                this.checkBoxEnableLoveChecked = SettingsClient.Get<bool>("Behaviour", "EnableLove");
                this.checkBoxShowRemoveFromDiskChecked = SettingsClient.Get<bool>("Behaviour", "ShowRemoveFromDisk");
                this.checkBoxSaveRatingInAudioFilesChecked = SettingsClient.Get<bool>("Behaviour", "SaveRatingToAudioFiles");
                this.checkBoxUseLogarithmicVolumeScaleChecked = SettingsClient.Get<bool>("Behaviour", "UseLogarithmicVolumeScale");
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
    }
}
