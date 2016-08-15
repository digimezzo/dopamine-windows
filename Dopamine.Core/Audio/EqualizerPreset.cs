using System.Collections.Generic;

namespace Dopamine.Core.Audio
{

    public class EqualizerPreset
    {
        #region Variables
        private string name;
        private bool isRemovable;
        #endregion

        #region Properties
        public double[] Bands { get; set; }
     
        public bool IsRemovable
        {
            get { return this.isRemovable; }
        }

        public string Name
        {
            get { return this.name; }
        }
        #endregion

        #region Construction
        public EqualizerPreset(string name, bool isRemovable)
        {
            this.name = name;
            this.isRemovable = isRemovable;
            this.LoadDefault();
        }
        #endregion

        #region Public
        public void Load(double[] bands)
        {
            if (bands != null && bands.Length == 10)
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

            for (int i = 0; i < 10; i++)
            {
                bandsList.Add(0.0);
            }

            this.Bands = bandsList.ToArray();
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return this.name;
        }
        #endregion
    }
}
