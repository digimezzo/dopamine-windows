using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Core.Audio
{

    public class EqualizerPreset
    {
        #region Variables
        private int size;
        private string name;
        #endregion

        #region Properties
        public double[] Bands { get; set; }
        #endregion

        #region Construction
        public EqualizerPreset(string name, int size)
        {
            this.size = size;
        }
        #endregion

        #region Public
        public void Load(double[] bands)
        {
            if (bands != null && bands.Length == size)
            {
                this.Bands = bands;
            }
            else
            {
                this.LoadDefault();
            }
        }

        public void LoadDefault()
        {
            var bandsList = new List<double>();

            for (int i = 0; i < size; i++)
            {
                bandsList.Add(0.0);
            }

            this.Bands = bandsList.ToArray();
        }
        #endregion
    }
}
