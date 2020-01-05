using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Dopamine.Services.Playback;
using Dopamine.Services.Win32Input;
using System;

namespace Dopamine.Services.Win32Input
{
    public class Win32InputService : IWin32InputService
    {
        private IPlaybackService playbackService;
        private IKeyboardHookManager lowLevelManager;
        private IKeyboardHookManager appCommandManager;

        public Win32InputService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "MediaKeys", "UseAppCommandMediaKeys"))
                {
                    this.SetEventHandlers();
                }

                if (SettingsClient.IsSettingChanged(e, "Behaviour", "EnableSystemNotification"))
                {
                    this.SetEventHandlers();
                }
            };
        }

        public void SetKeyboardHook(IntPtr hWnd)
        {
#if DEBUG
            // Set keyboard hook only when not debugging, because it slows down jumping through code using the keyboard.
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            if (this.lowLevelManager == null)
            {
                this.lowLevelManager = new LowLevelKeyboardHookManager();
            }

            this.lowLevelManager.SetHook();

            if (this.appCommandManager == null)
            {
                this.appCommandManager = new AppCommandKeyboardHookManager(hWnd);
            }

            this.appCommandManager.SetHook();

            this.SetEventHandlers();
#pragma warning restore CS0162 // Unreachable code detected
        }

        public void UnhookKeyboard()
        {
#if DEBUG
            // Set keyboard hook only when not debugging, because it slows down jumping through code using the keyboard.
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            this.RemoveEventHandlers();

            if (this.lowLevelManager != null)
            {
                this.lowLevelManager.Unhook();
            }

            if (this.appCommandManager != null)
            {
                this.appCommandManager.Unhook();
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        private async void MediaKeyNextPressedHandler(object sender, EventArgs e)
        {
            await this.playbackService.PlayNextAsync();
        }

        private async void MediaKeyPreviousPressedHandler(object sender, EventArgs e)
        {
            await this.playbackService.PlayPreviousAsync();
        }

        private async void MediaKeyPlayPressedHandler(object sender, EventArgs e)
        {
            await this.playbackService.PlayOrPauseAsync();
        }

        private void SetEventHandlers()
        {
            this.RemoveEventHandlers();

            // Only enable our own media keys support when not using system notifications.
            // System notifications have their own media key support, which interferes with ours.
            if (!SettingsClient.Get<bool>("Behaviour", "EnableSystemNotification"))
            {
                if (SettingsClient.Get<bool>("MediaKeys", "UseAppCommandMediaKeys"))
                {
                    LogClient.Info("Using AppCommand media keys");
                    this.appCommandManager.MediaKeyPlayPressed += MediaKeyPlayPressedHandler;
                    this.appCommandManager.MediaKeyPreviousPressed += MediaKeyPreviousPressedHandler;
                    this.appCommandManager.MediaKeyNextPressed += MediaKeyNextPressedHandler;
                }
                else
                {
                    LogClient.Info("Using LowLevel media keys");
                    this.lowLevelManager.MediaKeyPlayPressed += MediaKeyPlayPressedHandler;
                    this.lowLevelManager.MediaKeyPreviousPressed += MediaKeyPreviousPressedHandler;
                    this.lowLevelManager.MediaKeyNextPressed += MediaKeyNextPressedHandler;
                }
            }
        }

        private void RemoveEventHandlers()
        {
            if (this.lowLevelManager != null)
            {
                this.lowLevelManager.MediaKeyPlayPressed -= MediaKeyPlayPressedHandler;
                this.lowLevelManager.MediaKeyPreviousPressed -= MediaKeyPreviousPressedHandler;
                this.lowLevelManager.MediaKeyNextPressed -= MediaKeyNextPressedHandler;
            }

            if (this.appCommandManager != null)
            {
                this.appCommandManager.MediaKeyPlayPressed -= MediaKeyPlayPressedHandler;
                this.appCommandManager.MediaKeyPreviousPressed -= MediaKeyPreviousPressedHandler;
                this.appCommandManager.MediaKeyNextPressed -= MediaKeyNextPressedHandler;
            }
        }
    }
}
