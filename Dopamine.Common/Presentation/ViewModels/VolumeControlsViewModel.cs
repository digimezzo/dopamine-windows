using Dopamine.Common.Services.Playback;
using Dopamine.Core.Settings;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class VolumeControlsViewModel : BindableBase
    {
        #region Variables
        private IPlaybackService playBackService;
        private float volumeValue;
        private bool mute;
        #endregion

        #region Commands
        public DelegateCommand MuteCommand { get; set; }
        public DelegateCommand UnmuteCommand { get; set; }
        #endregion

        #region Properties
        public float VolumeValue
        {
            get { return this.volumeValue; }
            // We misuse the Property Setter to only set the PlayBackService Volume.
            // OnPropertyChanged is fired by the returning PlayBackService.PlaybackVolumeChanged event.
            // This prevents a StackOverflow (infinite loop between the VolumeValue Property and the 
            // PlayBackService.PlaybackVolumeChanged event.
            set { this.SetPlayBackServiceVolume(value); }
        }

        public int VolumeValuePercent
        {
            get { return Convert.ToInt32(this.VolumeValue * 100); }
        }

        public bool Mute
        {
            get { return this.mute; }

            set
            {
                // Empty on purpose. OnPropertyChanged is fired in GetPlaybackServiceMute.
            }
        }
        #endregion

        #region Construction
        public VolumeControlsViewModel(IPlaybackService playBackService)
        {
            this.playBackService = playBackService;

            this.playBackService.PlaybackVolumeChanged += (_, __) => this.GetPlayBackServiceVolume();
            this.playBackService.PlaybackMuteChanged += (_, __) => this.GetPlaybackServiceMute();

            this.MuteCommand = new DelegateCommand(() => this.SetPlayBackServiceMute(true));
            this.UnmuteCommand = new DelegateCommand(() => this.SetPlayBackServiceMute(false));

            // Set initial volume
            // ------------------

            // Important: set volumeValue directly, not the VolumeValue 
            // Property, because the VolumeValue Property Setter is empty!
            this.volumeValue = XmlSettingsClient.Instance.Get<float>("Playback", "Volume");
            this.SetPlayBackServiceVolume(this.volumeValue);

            // Set initial mute
            // ------------------

            // Important: set mute directly, not the Mute 
            // Property, because the Mute Property Setter is empty!
            this.mute = XmlSettingsClient.Instance.Get<bool>("Playback", "Mute");

            this.playBackService.SetMute(this.Mute);
        }
        #endregion

        #region Private
        private void SetPlayBackServiceVolume(float iVolume)
        {
            this.playBackService.Volume = iVolume;
        }

        private void GetPlayBackServiceVolume()
        {
            // Important: set volumeValue directly, not the VolumeValue 
            // Property, because the VolumeValue Property Setter is empty!
            this.volumeValue = this.playBackService.Volume;
            OnPropertyChanged(() => this.VolumeValue);
            OnPropertyChanged(() => this.VolumeValuePercent);

            // Save the volume in the Settings
            XmlSettingsClient.Instance.Set<double>("Playback", "Volume", Math.Round(this.volumeValue, 2));
        }

        private void SetPlayBackServiceMute(bool iMute)
        {
            this.playBackService.SetMute(iMute);
        }


        private void GetPlaybackServiceMute()
        {
            // Important: set mute directly, not the Mute 
            // Property, because the Mute Property Setter is empty!
            this.mute = this.playBackService.Mute;
            OnPropertyChanged(() => this.Mute);

            // Save the mute value in the Settings
            XmlSettingsClient.Instance.Set<bool>("Playback", "Mute", this.mute);
        }
        #endregion
    }

}