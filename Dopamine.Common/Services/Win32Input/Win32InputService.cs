using Dopamine.Core.Logging;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Dopamine.Common.Services.Win32Input
{
    /// <summary>
    /// See: http://www.markodevcic.com/post/.NET_Keyboard_Hook/
    /// </summary> 
    public class Win32InputService : IWin32InputService
    {
        #region Enums
        private enum MediaKey
        {
            VolumeDown = 174,
            VolumeUp = 175,
            Next = 176,
            Previous = 177,
            Stop = 178,
            Play = 179
        }
        #endregion

        #region Delegates
        public delegate IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam);
        #endregion

        #region IWin32InputService
        public void SetKeyboardHook(IntPtr hwnd)
        {
            /*
            this.hookCallBack = new KeyboardHookCallback(HookCallback);

            using (Process process = Process.GetCurrentProcess())
            {
                using (ProcessModule module = process.MainModule)
                {
                    this.keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, this.hookCallBack, GetModuleHandle(module.ModuleName), 0);
                }
            }
            */
            StartHook(hwnd);

        }

        public void UnhookKeyboard()
        {
            //UnhookWindowsHookEx(keyboardHookID);
            StopHook();
        }
        #endregion

        #region Low-level keyboard hook
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;
        private IntPtr keyboardHookID;
        private KeyboardHookCallback hookCallBack;

        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && (wParam.ToInt32() == WM_KEYUP || wParam.ToInt32() == WM_SYSKEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == (int)MediaKey.Next)
                {
                    if (MediaKeyNextPressed != null)
                    {
                        MediaKeyNextPressed(this, null);
                    }
                }
                else if (vkCode == (int)MediaKey.Previous)
                {
                    if (MediaKeyPreviousPressed != null)
                    {
                        MediaKeyPreviousPressed(this, null);
                    }
                }
                else if (vkCode == (int)MediaKey.Play)
                {
                    if (MediaKeyPlayPressed != null)
                    {
                        MediaKeyPlayPressed(this, null);
                    }
                }
            }
            return CallNextHookEx(keyboardHookID, code, wParam, lParam);
        }
        #endregion

        #region DLL imports
        [DllImport("user32")]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        #region Events
        public event EventHandler MediaKeyNextPressed;
        public event EventHandler MediaKeyPreviousPressed;
        public event EventHandler MediaKeyPlayPressed;
        #endregion



        /// <summary>
        /// Note:Some Media Keys get translated into APP_COMMAND Windows messages.
        /// reference : http://stackoverflow.com/questions/14087873/how-to-hook-global-wm-appcommand-message
        /// </summary> 
        #region Hook Shell AppCommand
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DeregisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);

        private static int WM_SHELLHOOKMESSAGE;
        private const int HSHELL_APPCOMMAND = 12;
        private const uint FAPPCOMMAND_MASK = 0xF000;

        private IntPtr _hWnd;
        private HwndSource _source;

        public enum Command
        {
            APPCOMMAND_MEDIA_NEXTTRACK = 11,
            APPCOMMAND_MEDIA_PAUSE = 47,
            APPCOMMAND_MEDIA_PLAY = 46,
            APPCOMMAND_MEDIA_PLAY_PAUSE = 14,
            APPCOMMAND_MEDIA_PREVIOUSTRACK = 12,
        }

        private void StartHook(IntPtr hWnd)
        {
            this._hWnd = hWnd;
            if (_source == null)
            {
                _source = HwndSource.FromHwnd(hWnd);
                if (_source == null)
                {
                    LogClient.Instance.Logger.Error("hWnd is NULL.");
                }
                _source.AddHook(WndProc);
                WM_SHELLHOOKMESSAGE = (int)RegisterWindowMessage("SHELLHOOK");
                if (WM_SHELLHOOKMESSAGE == 0)
                {
                    LogClient.Instance.Logger.Error("RegisterWindowMessage 'SHELLHOOK' failed.");

                }
                if (!RegisterShellHookWindow(_hWnd))
                {
                    LogClient.Instance.Logger.Error("RegisterShellHookWindow failed.");
                }
            }
        }


        private void StopHook()
        {
            if (_source != null)
            {
                _source.RemoveHook(WndProc);
                if (!_source.IsDisposed)
                {
                    if (!DeregisterShellHookWindow(_hWnd))
                    {
                        LogClient.Instance.Logger.Error("DeregisterShellHookWindow failed.");
                    }
                    _source.Dispose();
                }
                _source = null;
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
                        if (MediaKeyNextPressed != null)
                        {
                            MediaKeyNextPressed(this, null);
                        }
                        break;
                    case Command.APPCOMMAND_MEDIA_PAUSE:
                    case Command.APPCOMMAND_MEDIA_PLAY:
                    case Command.APPCOMMAND_MEDIA_PLAY_PAUSE:
                        if (MediaKeyPlayPressed != null)
                        {
                            MediaKeyPlayPressed(this, null);
                        }
                        break;
                    case Command.APPCOMMAND_MEDIA_PREVIOUSTRACK:
                        if (MediaKeyPreviousPressed != null)
                        {
                            MediaKeyPreviousPressed(this, null);
                        }
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

    }
}
