using Dopamine.Core.Prism;
using Dopamine.Core.Settings;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
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
        private bool checkBoxUseStarRatingChecked;
        private bool checkBoxSaveRatingInAudioFilesChecked;
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

        public bool CheckBoxUseStarRatingChecked
        {
            get { return this.checkBoxUseStarRatingChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Behaviour", "UseStarRating", value);
                SetProperty<bool>(ref this.checkBoxUseStarRatingChecked, value);
                Application.Current.Dispatcher.Invoke(() => this.eventAggregator.GetEvent<SettingUseStarRatingChanged>().Publish(value));
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
        #endregion

        #region Construction
        public SettingsBehaviourViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.GetCheckBoxesAsync();
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
                this.CheckBoxUseStarRatingChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "UseStarRating");
                this.CheckBoxSaveRatingInAudioFilesChecked = XmlSettingsClient.Instance.Get<bool>("Behaviour", "SaveRatingToAudioFiles");
            });
        }
        #endregion
    }
}
