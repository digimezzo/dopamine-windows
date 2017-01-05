using System;
using System.Runtime.InteropServices;

namespace Dopamine.Core.Win32
{
    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern Int32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        [DllImport("user32.dll")]
        static internal extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        #region SHFileOperation
        private const int FO_DELETE = 3;
        private const int FOF_ALLOWUNDO = 40;
        private const int FOF_NOCONFIRMATION = 10;
        private const int FOF_SILENT = 44;
        private const int FOF_NOERRORUI = 400;
        private const int FOF_NOCONFIRMMKDIR = 0200;
        private const int FOF_NO_UI = FOF_SILENT | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_NOCONFIRMMKDIR;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        public static void RemoveFileToRecycleBin(string file)
        {
            var shf = new SHFILEOPSTRUCT();
            shf.wFunc = FO_DELETE;
            shf.fFlags = FOF_NO_UI;
            shf.pFrom = file;
            SHFileOperation(ref shf);
        }
        #endregion
    }
}
