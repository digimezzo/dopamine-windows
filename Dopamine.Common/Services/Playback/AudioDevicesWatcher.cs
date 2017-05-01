using CSCore.CoreAudioAPI;
using System;
using System.Timers;

namespace Dopamine.Common.Services.Playback
{
    internal class AudioDevicesWatcher
    {
        #region Variables
        private MMNotificationClient mmNotificationClient = new MMNotificationClient();
        private Timer audioDevicesChangedTimer = new Timer();
        #endregion

        #region Construction
        public AudioDevicesWatcher()
        {
            audioDevicesChangedTimer.Interval = 250;
            audioDevicesChangedTimer.Elapsed += AudioDevicesChangedTimer_Elapsed;
        }
        #endregion

        #region Events
        public event EventHandler AudioDevicesChanged = delegate { };
        #endregion

        #region Public
        public void StartWatching()
        {
            mmNotificationClient.DefaultDeviceChanged += MmNotificationClient_DefaultDeviceChanged;
            mmNotificationClient.DeviceRemoved += MmNotificationClient_DeviceRemoved;
            mmNotificationClient.DeviceStateChanged += MmNotificationClient_DeviceStateChanged;
            mmNotificationClient.DeviceAdded += MmNotificationClient_DeviceAdded;
        }

        public void StopWatching()
        {
            mmNotificationClient.DefaultDeviceChanged -= MmNotificationClient_DefaultDeviceChanged;
            mmNotificationClient.DeviceRemoved -= MmNotificationClient_DeviceRemoved;
            mmNotificationClient.DeviceStateChanged -= MmNotificationClient_DeviceStateChanged;
            mmNotificationClient.DeviceAdded -= MmNotificationClient_DeviceAdded;
        }
        #endregion

        #region Event handlers
        private void AudioDevicesChangedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.audioDevicesChangedTimer.Stop();
            this.AudioDevicesChanged(this, new EventArgs());
        }

        private void MmNotificationClient_DeviceAdded(object sender, DeviceNotificationEventArgs e)
        {
            this.audioDevicesChangedTimer.Stop();
            this.audioDevicesChangedTimer.Start();
        }

        private void MmNotificationClient_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            this.audioDevicesChangedTimer.Stop();
            this.audioDevicesChangedTimer.Start();
        }

        private void MmNotificationClient_DeviceRemoved(object sender, DeviceNotificationEventArgs e)
        {
            this.audioDevicesChangedTimer.Stop();
            this.audioDevicesChangedTimer.Start();
        }

        private void MmNotificationClient_DefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
        {
            this.audioDevicesChangedTimer.Stop();
            this.audioDevicesChangedTimer.Start();
        }
        #endregion
    }
}