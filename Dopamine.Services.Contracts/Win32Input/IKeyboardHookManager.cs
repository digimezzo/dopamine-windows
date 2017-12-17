using System;

namespace Dopamine.Services.Contracts.Win32Input
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
