using System;

namespace Dopamine.Services.Win32Input
{
    public interface IKeyboardHookManager
    {
        void SetHook();
        void Unhook();

        event EventHandler MediaKeyNextPressed;
        event EventHandler MediaKeyPreviousPressed;
        event EventHandler MediaKeyPlayPressed;
    }
}
