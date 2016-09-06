using Dopamine.Core.Logging;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Dopamine.Common.Services.Win32Input
{
    /// <summary>
    /// Low-Level Keyboard Hook, see: http://www.markodevcic.com/post/.NET_Keyboard_Hook/
    /// APPCOMMAND Keyboard Hook (because some Media Keys get translated into APP_COMMAND Windows messages), 
    /// see: http://stackoverflow.com/questions/14087873/how-to-hook-global-wm-appcommand-message
    /// </summary> 
    public class Win32InputService : IWin32InputService
    {
        #region Low-Level Keyboard Hook
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;
        private IntPtr keyboardHookID;
        private KeyboardHookCallback hookCallBack;

        private enum MediaKey
        {
            VolumeDown = 174,
            VolumeUp = 175,
            Next = 176,
            Previous = 177,
            Stop = 178,
            Play = 179
        }

        public delegate IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam);

        private void SetLowLevelKeyboardHook()
        {
            this.hookCallBack = new KeyboardHookCallback(HookCallback);

            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    this.keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, this.hookCallBack, GetModuleHandle(module.ModuleName), 0);
                }
            }
        }

        private void UnHookLowLevelKeyboardHook()
        {
            UnhookWindowsHookEx(keyboardHookID);
        }

        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && (wParam.ToInt32() == WM_KEYUP || wParam.ToInt32() == WM_SYSKEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == (int)MediaKey.Next)
                {
                    this.MediaKeyNextPressed(this, null);
                }
                else if (vkCode == (int)MediaKey.Previous)
                {
                    this.MediaKeyPreviousPressed(this, null);
                }
                else if (vkCode == (int)MediaKey.Play)
                {
                    this.MediaKeyPlayPressed(this, null);
                }
            }
            return CallNextHookEx(keyboardHookID, code, wParam, lParam);
        }

        [DllImport("user32")]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        #region APPCOMMAND Keyboard Hook
        private static int WM_SHELLHOOKMESSAGE;
        private const int HSHELL_APPCOMMAND = 12;
        private const uint FAPPCOMMAND_MASK = 0xF000;
        private HwndSource source;
        private IntPtr hWnd;

        public enum Command
        {
            APPCOMMAND_MEDIA_NEXTTRACK = 11,
            APPCOMMAND_MEDIA_PAUSE = 47,
            APPCOMMAND_MEDIA_PLAY = 46,
            APPCOMMAND_MEDIA_PLAY_PAUSE = 14,
            APPCOMMAND_MEDIA_PREVIOUSTRACK = 12,
        }

        private void SetAppCommandKeyboardHook(IntPtr hWnd)
        {
            this.hWnd = hWnd;

            if (this.source == null)
            {
                this.source = HwndSource.FromHwnd(this.hWnd);

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

        private void UnHookAppCommandKeyboardHook()
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

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHELLHOOKMESSAGE && (int)wParam == HSHELL_APPCOMMAND)
            {
                var command = GetAppCommandLParam(lParam);

                switch (command)
                {
                    case Command.APPCOMMAND_MEDIA_NEXTTRACK:
                        this.MediaKeyNextPressed(this, new EventArgs());
                        break;
                    case Command.APPCOMMAND_MEDIA_PAUSE:
                    case Command.APPCOMMAND_MEDIA_PLAY:
                    case Command.APPCOMMAND_MEDIA_PLAY_PAUSE:
                        this.MediaKeyPlayPressed(this, new EventArgs());
                        break;
                    case Command.APPCOMMAND_MEDIA_PREVIOUSTRACK:
                        this.MediaKeyPreviousPressed(this, new EventArgs());
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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DeregisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);
        #endregion

        #region IWin32InputService
        public event EventHandler MediaKeyNextPressed = delegate { };
        public event EventHandler MediaKeyPreviousPressed = delegate { };
        public event EventHandler MediaKeyPlayPressed = delegate { };

        public void SetKeyboardHook(IntPtr hWnd)
        {
            this.SetLowLevelKeyboardHook();
            this.SetAppCommandKeyboardHook(hWnd);
        }

        public void UnhookKeyboard()
        {
            this.UnHookLowLevelKeyboardHook();
            this.UnHookAppCommandKeyboardHook();
        }
        #endregion
    }
}
