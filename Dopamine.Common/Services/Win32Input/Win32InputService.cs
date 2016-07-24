using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
        public void SetKeyboardHook()
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

        public void UnhookKeyboard()
        {
            UnhookWindowsHookEx(keyboardHookID);
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
    }
}
