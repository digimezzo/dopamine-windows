using Dopamine.Core.Base;
using System;
using System.Windows;

namespace Dopamine.Core.Extensions
{
    public static class WindowExtensions
    {
        public static void SetGeometry(this Window win, double top, double left, double width, double height)
        {
            if (left <= (SystemParameters.VirtualScreenLeft - width) | top <= (SystemParameters.VirtualScreenTop - height) | (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth) <= left | (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight) <= top)
            {
                top = Convert.ToInt32(Constants.DefaultShellTop);
                left = Convert.ToInt32(Constants.DefaultShellLeft);
            }

            win.Top = top;
            win.Left = left;
            win.Width = width;
            win.Height = height;
        }
    }
}
