using CSCore.CoreAudioAPI;
using System;
using System.Timers;

namespace Dopamine.Common.Services.Playback
{
    internal class AudioDevicesWatcher
    {
        private MMNotificationClient mmNotificationClient = new MMNotificationClient();
        private Timer audioDevicesChangedTimer = new Timer();
  
        public AudioDevicesWatcher()
        {
            audioDevicesChangedTimer.Interval = 250;
            audioDevicesChangedTimer.Elapsed += AudioDevicesChangedTimer_Elapsed;
        }
   
        public event EventHandler AudioDevicesChanged = delegate { };
       
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
    }
}