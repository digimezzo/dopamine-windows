using Prism.Mvvm;

namespace Dopamine.Core.Audio
{
    public delegate void ValueChangedEventHandler(string bandLabel, double newValue);

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
                this.ValueChanged(this.label, value);
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

        #region Events
        public event ValueChangedEventHandler ValueChanged = delegate { };
        #endregion
    }
}
