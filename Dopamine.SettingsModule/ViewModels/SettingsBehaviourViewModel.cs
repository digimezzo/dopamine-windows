using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
using Prism.Mvvm;
using Prism.Events;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using Dopamine.Core.Helpers;
using System.Linq;

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
        private ObservableCollection<NameValue> scrollVolumePercentages;
        private NameValue selectedScrollVolumePercentage;
        #endregion

        #region Properties
        public bool CheckBoxShowTrayIconChecked
        {
            get { return this.checkBoxShowTrayIconChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "ShowTrayIcon", value);
                SetProperty<bool>(ref this.checkBoxShowTrayIconChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingShowTrayIconChanged>().Publish(value));
            }
        }

        public bool CheckBoxMinimizeToTrayChecked
        {
            get { return this.checkBoxMinimizeToTrayChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "MinimizeToTray", value);
                SetProperty<bool>(ref this.checkBoxMinimizeToTrayChecked, value);
            }
        }

        public bool CheckBoxCloseToTrayChecked
        {
            get { return this.checkBoxCloseToTrayChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "CloseToTray", value);
                SetProperty<bool>(ref this.checkBoxCloseToTrayChecked, value);
            }
        }

        public bool CheckBoxFollowTrackChecked
        {
            get { return this.checkBoxFollowTrackChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "FollowTrack", value);
                SetProperty<bool>(ref this.checkBoxFollowTrackChecked, value);
            }
        }

        public bool CheckBoxEnableRatingChecked
        {
            get { return this.checkBoxEnableRatingChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "EnableRating", value);
                SetProperty<bool>(ref this.checkBoxEnableRatingChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingEnableRatingChanged>().Publish(value));
            }
        }

        public bool CheckBoxEnableLoveChecked
        {
            get { return this.checkBoxEnableLoveChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "EnableLove", value);
                SetProperty<bool>(ref this.checkBoxEnableLoveChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingEnableLoveChanged>().Publish(value));
            }
        }

        public bool CheckBoxSaveRatingInAudioFilesChecked
        {
            get { return this.checkBoxSaveRatingInAudioFilesChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "SaveRatingToAudioFiles", value);
                SetProperty<bool>(ref this.checkBoxSaveRatingInAudioFilesChecked, value);
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
                XmlSettingsClient.Instance.Set<int>("Behaviour", "ScrollVolumePercentage", value.Value);
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
                this.CheckBoxShowTrayIconChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "ShowTrayIcon");
                this.CheckBoxMinimizeToTrayChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "MinimizeToTray");
                this.CheckBoxCloseToTrayChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "CloseToTray");
                this.CheckBoxFollowTrackChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "FollowTrack");
                this.CheckBoxEnableRatingChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "EnableRating");
                this.CheckBoxEnableLoveChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "EnableLove");
                this.CheckBoxSaveRatingInAudioFilesChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "SaveRatingToAudioFiles");
            });
        }

        private async void GetScrollVolumePercentagesAsync()
        {
            var localScrollVolumePercentages = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localScrollVolumePercentages.Add(new NameValue { Name = "1 %", Value = 1 });
                localScrollVolumePercentages.Add(new NameValue { Name = "2 %", Value = 2 });
                localScrollVolumePercentages.Add(new NameValue { Name = "5 % (" + Application.Current.FindResource("Language_Default").ToString().ToLower() + ")", Value = 5 });
                localScrollVolumePercentages.Add(new NameValue { Name = "10 %", Value = 10 });
                localScrollVolumePercentages.Add(new NameValue { Name = "15 %", Value = 15 });
                localScrollVolumePercentages.Add(new NameValue { Name = "20 %", Value = 20 });
            });

            this.ScrollVolumePercentages = localScrollVolumePercentages;

            NameValue localSelectedScrollVolumePercentage = null;
            await Task.Run(() => localSelectedScrollVolumePercentage = this.ScrollVolumePercentages.Where((svp) => svp.Value == XmlSettingsClient.Instance.Get<int>("Behaviour", "ScrollVolumePercentage")).Select((svp) => svp).First());
            this.SelectedScrollVolumePercentage = localSelectedScrollVolumePercentage;
        }
        #endregion
    }
}
