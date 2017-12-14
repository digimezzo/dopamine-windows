using System.Diagnostics;
using System.Windows.Media;

namespace Dopamine.Core.Utils
{
    /// <summary>
    /// Same as MS.Internal.PresentationCore
    /// </summary>
    [DebuggerStepThrough]
    public static class ColorUtils
    {
        public static Color InterpolateColor(Color from, Color to, double progress)
        {
            return from + ((to - from) * (float)progress);
        }
    }
}