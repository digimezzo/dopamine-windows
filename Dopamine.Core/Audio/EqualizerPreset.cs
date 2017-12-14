using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dopamine.Core.Audio
{
    public delegate void BandValueChangedEventHandler(int bandIndex, double newValue);

    public class EqualizerPreset
    {
        private double[] bands;
        private string name;
        private bool isRemovable;

        public double[] Bands
        {
            get { return this.bands; }
            set
            {
                this.bands = value;
            }
        }

        public bool IsRemovable
        {
            get { return this.isRemovable; }
        }

        public string Name
        {
            get { return this.name; }
        }

        private string displayName;

        public string DisplayName
        {
            get { return this.displayName; }
            set
            {
                this.displayName = value;
            }
        }

        public EqualizerPreset(string name, bool isRemovable)
        {
            this.name = name;
            this.DisplayName = name;
            this.isRemovable = isRemovable;
            this.Initialize();
        }

        public EqualizerPreset()
        {
            this.name = Guid.NewGuid().ToString();
            this.DisplayName = name;
            this.isRemovable = false;
            this.Initialize();
        }

        private void Initialize()
        {
            var bandsList = new List<double>();

            for (int i = 0; i < 10; i++)
            {
                bandsList.Add(0.0);
            }

            this.Bands = bandsList.ToArray();
        }

        public void Load(double[] values)
        {
            for (int i = 0; i < values.Count(); i++)
            {
                this.Bands[i] = values[i];
            }
        }

        public void Load(double zero, double one, double two, double three, double four, double five, double six, double seven, double eight, double nine)
        {
            this.Bands[0] = zero;
            this.Bands[1] = one;
            this.Bands[2] = two;
            this.Bands[3] = three;
            this.Bands[4] = four;
            this.Bands[5] = five;
            this.Bands[6] = six;
            this.Bands[7] = seven;
            this.Bands[8] = eight;
            this.Bands[9] = nine;
        }

        public string ToValueString()
        {
            return string.Join(";", Array.ConvertAll<double, string>(this.Bands, s => s.ToString(CultureInfo.InvariantCulture)));
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.name.Equals(((EqualizerPreset)obj).name);
        }

        public override int GetHashCode()
        {
            return new { this.name }.GetHashCode();
        }
    }
}