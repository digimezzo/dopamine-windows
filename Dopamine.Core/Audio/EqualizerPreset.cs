using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Core.Audio
{

    public class EqualizerPreset
    {
        #region Properties
        public double[] Bands { get; set; }
        #endregion

        #region Construction
        public EqualizerPreset(double[] bands)
        {
            this.Bands = bands;
        }
        #endregion
    }
}
