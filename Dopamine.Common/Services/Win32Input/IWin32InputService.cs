using System;
using System.Windows.Interop;

namespace Dopamine.Common.Services.Win32Input
{
    public interface IWin32InputService
    {
        void RestoreFromAppSwither(WindowInteropHelper wndHelper);
        void RemoveFromAppSwither(WindowInteropHelper wndHelper);
        void SetKeyboardHook(IntPtr hwnd);
        void UnhookKeyboard();
        event EventHandler MediaKeyNextPressed;
        event EventHandler MediaKeyPreviousPressed;
        event EventHandler MediaKeyPlayPressed;
    }
}
