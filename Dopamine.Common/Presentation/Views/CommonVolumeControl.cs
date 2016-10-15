using Dopamine.Core.Settings;
using System;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public class CommonVolumeControl : UserControl
    {
        #region Protected
        protected double CalculateVolumeDelta(double scrollDelta)
        {
            int scrollVolumePercentage = XmlSettingsClient.Instance.Get<int>("Behaviour", "ScrollVolumePercentage");
            return (double)scrollVolumePercentage / 100 * Math.Sign(scrollDelta);
        }
        #endregion
    }
}
