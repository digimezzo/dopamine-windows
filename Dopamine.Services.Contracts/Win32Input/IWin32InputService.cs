using System;

namespace Dopamine.Services.Contracts.Win32Input
{
    public interface IWin32InputService
    {
        void SetKeyboardHook(IntPtr hwnd);
        void UnhookKeyboard();
    }
}
