using Digimezzo.Utilities.Log;
using Dopamine.Services.Win32Input;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dopamine.Services.Win32Input
{
    /// <summary>
    /// Low-Level Keyboard Hook, see: http://www.markodevcic.com/post/.NET_Keyboard_Hook/
    /// </summary> 
    internal class LowLevelKeyboardHookManager : IKeyboardHookManager
    {
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

        public event EventHandler MediaKeyNextPressed = delegate { };
        public event EventHandler MediaKeyPreviousPressed = delegate { };
        public event EventHandler MediaKeyPlayPressed = delegate { };

        public LowLevelKeyboardHookManager()
        {
        }

        public void SetHook()
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

        public void Unhook()
        {
            UnhookWindowsHookEx(keyboardHookID);
        }

        [DebuggerHidden]
        private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && (wParam.ToInt32() == WM_KEYUP || wParam.ToInt32() == WM_SYSKEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == (int)MediaKey.Next)
                {
                    LogClient.Info("LowLevel Key Next pressed");
                    this.MediaKeyNextPressed(this, new EventArgs());
                }
                else if (vkCode == (int)MediaKey.Previous)
                {
                    LogClient.Info("LowLevel Key Previous pressed");
                    this.MediaKeyPreviousPressed(this, new EventArgs());
                }
                else if (vkCode == (int)MediaKey.Play)
                {
                    LogClient.Info("LowLevel Key Play pressed");
                    this.MediaKeyPlayPressed(this, new EventArgs());
                }
            }
            return CallNextHookEx(keyboardHookID, code, wParam, lParam);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32")]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    }
}
