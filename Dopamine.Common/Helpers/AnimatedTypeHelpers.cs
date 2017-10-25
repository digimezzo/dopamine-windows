using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Dopamine.Common.Helpers
{
    /// <summary>
    /// Same as MS.Internal.PresentationCore
    /// </summary>
    [DebuggerStepThrough]
    public static class AnimatedTypeHelpers
    {
        public static Color InterpolateColor(Color from, Color to, double progress)
        {
            return from + ((to - from) * (float)progress);
        }
    }
}