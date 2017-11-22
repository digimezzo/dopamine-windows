using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Playback;
using System;
using System.Timers;

namespace Dopamine.Common.Services.Win32Input
{
    public class Win32InputService : IWin32InputService
    {
        private IPlaybackService playbackService;
        private bool isMediaKeyJustPressed = false;
        private Timer canRaiseMediaKeyEventTimer = new Timer();
        private IKeyboardHookManager lowLevelManager ;
        private IKeyboardHookManager appCommandManager;

        public Win32InputService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
            this.canRaiseMediaKeyEventTimer.Interval = 250;
            this.canRaiseMediaKeyEventTimer.Elapsed += CanPressMediaKeyTimer_Elapsed;
        }

        public void SetKeyboardHook(IntPtr hWnd)
        {
#if DEBUG
            // Set keyboard hook only when not debugging, because it slows down jumping through code using the keyboard.
            return;
#endif
            if (this.lowLevelManager == null)
            {
                this.lowLevelManager = new LowLevelKeyboardHookManager();
            }

            if (this.appCommandManager == null)
            {
                this.appCommandManager = new AppCommandKeyboardHookManager(hWnd);
            }

            this.lowLevelManager.MediaKeyPlayPressed += MediaKeyPlayPressedHandler;
            this.lowLevelManager.MediaKeyPreviousPressed += MediaKeyPreviousPressedHandler;
            this.lowLevelManager.MediaKeyNextPressed += MediaKeyNextPressedHandler;
            this.appCommandManager.MediaKeyPlayPressed += MediaKeyPlayPressedHandler;
            this.appCommandManager.MediaKeyPreviousPressed += MediaKeyPreviousPressedHandler;
            this.appCommandManager.MediaKeyNextPressed += MediaKeyNextPressedHandler;

            this.lowLevelManager.SetHook();
            this.appCommandManager.SetHook();
        }

        private async void MediaKeyNextPressedHandler(object sender, EventArgs e)
        {
            if (this.CanPressMediaKey())
            {
                await this.playbackService.PlayNextAsync();
            }

            this.StartCanPressMediaKeyTimer();
        }

        private async void MediaKeyPreviousPressedHandler(object sender, EventArgs e)
        {
            if (this.CanPressMediaKey())
            {
                await this.playbackService.PlayPreviousAsync();
            }

            this.StartCanPressMediaKeyTimer();
        }

        private async void MediaKeyPlayPressedHandler(object sender, EventArgs e)
        {
            if (this.CanPressMediaKey())
            {
                await this.playbackService.PlayOrPauseAsync();
            }

            this.StartCanPressMediaKeyTimer();
        }

        public void UnhookKeyboard()
        {
#if DEBUG
            // Set keyboard hook only when not debugging, because it slows down jumping through code using the keyboard.
            return;
#endif
            this.lowLevelManager.MediaKeyPlayPressed -= MediaKeyPlayPressedHandler;
            this.lowLevelManager.MediaKeyPreviousPressed -= MediaKeyPreviousPressedHandler;
            this.lowLevelManager.MediaKeyNextPressed -= MediaKeyNextPressedHandler;
            this.appCommandManager.MediaKeyPlayPressed -= MediaKeyPlayPressedHandler;
            this.appCommandManager.MediaKeyPreviousPressed -= MediaKeyPreviousPressedHandler;
            this.appCommandManager.MediaKeyNextPressed -= MediaKeyNextPressedHandler;

            this.lowLevelManager.Unhook();
            this.appCommandManager.Unhook();
        }

        private bool CanPressMediaKey()
        {
            return !isMediaKeyJustPressed && SettingsClient.Get<bool>("Behaviour", "EnableSystemNotification");
        }

        private void CanPressMediaKeyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            canRaiseMediaKeyEventTimer.Stop();
            isMediaKeyJustPressed = false;
        }

        private void StartCanPressMediaKeyTimer()
        {
            isMediaKeyJustPressed = true;
            canRaiseMediaKeyEventTimer.Start();
        }
    }
}
