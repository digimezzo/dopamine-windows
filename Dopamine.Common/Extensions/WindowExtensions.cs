using Dopamine.Common.Base;
using System;
using System.Windows;

namespace Dopamine.Common.Extensions
{
    public static class WindowExtensions
    {
        public static void SetGeometry(this Window win, double top, double left, double width, double height, double topFallback = 50, double leftFallback = 50)
        {
            if (left <= (SystemParameters.VirtualScreenLeft - width) ||
                top <= (SystemParameters.VirtualScreenTop - height) ||
                (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth) <= left ||
                (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight) <= top)
            {
                top = Convert.ToInt32(topFallback);
                left = Convert.ToInt32(leftFallback);
            }

            win.Top = top;
            win.Left = left;
            win.Width = width;
            win.Height = height;
        }
    }
}
