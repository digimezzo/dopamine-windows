using Prism.Mvvm;

namespace Dopamine.Core.Audio
{
    public class EqualizerBand : BindableBase
    {
        #region Variables
        private double value;
        private string label;
        #endregion

        #region Properties
        public double Value
        {
            get { return this.value; }
            set
            {
                SetProperty<double>(ref this.value, value);
                OnPropertyChanged(() => this.TextValue);
            }
        }

        public string TextValue
        {
            get
            {
                return this.value >= 0 ? this.value.ToString("+0.0") : this.value.ToString("0.0");
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
        }
        #endregion

        #region Construction
        public EqualizerBand(string label)
        {
            this.label = label;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return this.TextValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Label.Equals(((EqualizerBand)obj).Label);
        }

        public override int GetHashCode()
        {
            return new { this.Label }.GetHashCode();
        }
        #endregion
    }
}
