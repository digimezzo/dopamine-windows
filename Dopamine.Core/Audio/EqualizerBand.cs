using Prism.Mvvm;
using System;

namespace Dopamine.Core.Audio
{
    public class EqualizerBand : BindableBase
    {
        #region Variables
        private double value;
        #endregion

        #region Properties
        public double Value
        {
            get { return this.value; }
            set {
                SetProperty<double>(ref this.value, value);
                OnPropertyChanged(() => this.StringValue);
            }
        }
        
        public string StringValue
        {
            get {
                double roundedValue = Math.Round(this.Value, 1);
                return roundedValue > 0 ? "+" + roundedValue.ToString() : roundedValue.ToString();
            }
        }
        #endregion
    }
}
