using Dopamine.Core.Logging;
using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Dopamine.Common.Services.Win32Input
{
    /// <summary>
    /// Note: some Media Keys get translated into APP_COMMAND Windows messages.
    /// Reference : http://stackoverflow.com/questions/14087873/how-to-hook-global-wm-appcommand-message
    /// </summary> 
    public class Win32InputService : IWin32InputService
    {
        #region Variables
        private static int WM_SHELLHOOKMESSAGE;
        private const int HSHELL_APPCOMMAND = 12;
        private const uint FAPPCOMMAND_MASK = 0xF000;

        private IntPtr hWnd;
        private HwndSource source;
        #endregion

        #region Enums
        public enum Command
        {
            APPCOMMAND_MEDIA_NEXTTRACK = 11,
            APPCOMMAND_MEDIA_PAUSE = 47,
            APPCOMMAND_MEDIA_PLAY = 46,
            APPCOMMAND_MEDIA_PLAY_PAUSE = 14,
            APPCOMMAND_MEDIA_PREVIOUSTRACK = 12,
        }
        #endregion

        #region IWin32InputService
        public void SetKeyboardHook(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            if (this.source == null)
            {
                this.source = HwndSource.FromHwnd(hWnd);

                if (this.source == null)
                {
                    LogClient.Instance.Logger.Error("hWnd is NULL.");
                }

                this.source.AddHook(WndProc);
                WM_SHELLHOOKMESSAGE = (int)RegisterWindowMessage("SHELLHOOK");

                if (WM_SHELLHOOKMESSAGE == 0)
                {
                    LogClient.Instance.Logger.Error("RegisterWindowMessage 'SHELLHOOK' failed.");

                }

                if (!RegisterShellHookWindow(this.hWnd))
                {
                    LogClient.Instance.Logger.Error("RegisterShellHookWindow failed.");
                }
            }
        }

        public void UnhookKeyboard()
        {
            if (this.source != null)
            {
                this.source.RemoveHook(WndProc);

                if (!this.source.IsDisposed)
                {
                    if (!DeregisterShellHookWindow(this.hWnd))
                    {
                        LogClient.Instance.Logger.Error("DeregisterShellHookWindow failed.");
                    }
                    this.source.Dispose();
                }

                this.source = null;
            }
        }
        #endregion

        #region Events
        public event EventHandler MediaKeyNextPressed = delegate { };
        public event EventHandler MediaKeyPreviousPressed = delegate { };
        public event EventHandler MediaKeyPlayPressed = delegate { };
        #endregion

        #region Private
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHELLHOOKMESSAGE && (int)wParam == HSHELL_APPCOMMAND)
            {
                var command = GetAppCommandLParam(lParam);

                switch (command)
                {
                    case Command.APPCOMMAND_MEDIA_NEXTTRACK:
                        MediaKeyNextPressed(this, new EventArgs());
                        break;
                    case Command.APPCOMMAND_MEDIA_PAUSE:
                    case Command.APPCOMMAND_MEDIA_PLAY:
                    case Command.APPCOMMAND_MEDIA_PLAY_PAUSE:
                        this.MediaKeyPlayPressed(this, new EventArgs());
                        break;
                    case Command.APPCOMMAND_MEDIA_PREVIOUSTRACK:
                        MediaKeyPreviousPressed(this, new EventArgs());
                        break;
                    default:
                        break;
                }

                handled = false;
            }
            return IntPtr.Zero;
        }

        private Command GetAppCommandLParam(IntPtr lParam)
        {
            return (Command)((short)(((ushort)((((uint)lParam.ToInt64()) >> 16) & 0xffff)) & ~FAPPCOMMAND_MASK));
        }
        #endregion

        #region Dll imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DeregisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);
        #endregion
    }
}
