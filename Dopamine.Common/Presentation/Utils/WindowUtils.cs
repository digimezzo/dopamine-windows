using System;
using System.Windows;

namespace Dopamine.Common.Presentation.Utils
{
    public sealed class WindowUtils
    {
        public static void CenterWindow(Window win)
        {
            // This is a hack to get the Dialog to center on the application's main window
            try
            {
                if (Application.Current.MainWindow.IsVisible)
                {
                    win.Owner = Application.Current.MainWindow;
                    win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    // If the main window is not visible (like when the OOBE screen is visible),
                    // center the Dialog on the screen
                    win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                // The try catch should not be necessary. But added just in case.
                win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }
}
