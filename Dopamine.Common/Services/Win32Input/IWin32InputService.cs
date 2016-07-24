using System;

namespace Dopamine.Common.Services.Win32Input
{
    public interface IWin32InputService
    {
        void SetKeyboardHook();
        void UnhookKeyboard();
        event EventHandler MediaKeyNextPressed;
        event EventHandler MediaKeyPreviousPressed;
        event EventHandler MediaKeyPlayPressed;
    }
}
