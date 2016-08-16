using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            set {
                SetProperty<double>(ref this.value, value);
                OnPropertyChanged(() => this.TextValue);
            }
        }

        public string TextValue
        {
            get {
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
    }
}
