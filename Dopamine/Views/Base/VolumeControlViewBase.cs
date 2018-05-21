using Digimezzo.Utilities.Settings;
using System;
using System.Windows.Controls;

namespace Dopamine.Views.Base
{
    public class VolumeControlViewBase : UserControl
    {
        protected double CalculateVolumeDelta(double scrollDelta)
        {
            int scrollVolumePercentage = SettingsClient.Get<int>("Behaviour", "ScrollVolumePercentage");
            return (double)scrollVolumePercentage / 100 * Math.Sign(scrollDelta);
        }
    }
}
