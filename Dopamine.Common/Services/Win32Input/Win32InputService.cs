using System;
using System.Timers;

namespace Dopamine.Common.Services.Win32Input
{
    public class Win32InputService : IWin32InputService
    {
        private bool canRaiseMediaKeyEvent = true;
        private Timer canRaiseMediaKeyEventTimer = new Timer();
        private IKeyboardHookManager lowLevelManager ;
        private IKeyboardHookManager appCommandManager;

        public Win32InputService()
        {
            this.canRaiseMediaKeyEventTimer.Interval = 250;
            this.canRaiseMediaKeyEventTimer.Elapsed += CanPressMediaKeyTimer_Elapsed;
        }

        public event EventHandler MediaKeyNextPressed = delegate { };
        public event EventHandler MediaKeyPreviousPressed = delegate { };
        public event EventHandler MediaKeyPlayPressed = delegate { };

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

        private void MediaKeyNextPressedHandler(object sender, EventArgs e)
        {
            if (this.canRaiseMediaKeyEvent)
            {
                this.MediaKeyNextPressed(this, new EventArgs());
            }

            this.StartCanPressMediaKeyTimer();
        }

        private void MediaKeyPreviousPressedHandler(object sender, EventArgs e)
        {
            if (this.canRaiseMediaKeyEvent)
            {
                this.MediaKeyPreviousPressed(this, new EventArgs());
            }

            this.StartCanPressMediaKeyTimer();
        }

        private void MediaKeyPlayPressedHandler(object sender, EventArgs e)
        {
            if (this.canRaiseMediaKeyEvent)
            {
                this.MediaKeyPlayPressed(this, new EventArgs());
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

        private void CanPressMediaKeyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            canRaiseMediaKeyEventTimer.Stop();
            canRaiseMediaKeyEvent = true;
        }

        private void StartCanPressMediaKeyTimer()
        {
            canRaiseMediaKeyEvent = false;
            canRaiseMediaKeyEventTimer.Start();
        }
    }
}
