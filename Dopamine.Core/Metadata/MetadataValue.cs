using Microsoft.Practices.Prism.Mvvm;
using System.Linq;

namespace Dopamine.Core.Metadata
{
    public class MetadataValue : BindableBase
    {
        #region Private
        private string value;
        private string[] values;
        private bool isInitialValue = true;
        private bool isValueChanged;
        #endregion

        #region Readonly Properties
        public bool IsValueChanged
        {
            get { return this.isValueChanged; }
        }

        public bool IsNumeric
        {
            get
            {
                int parsedValue = 0;
                return this.IsValueChanged & !string.IsNullOrEmpty(this.Value) ? int.TryParse(this.Value, out parsedValue) ? parsedValue >= 0 : false : true;
            }
        }
        #endregion

        #region Properties
        public string Value
        {
            //Return If(mValue IsNot Nothing, mValue, String.Empty)
            get { return this.value; }

            set
            {
                // This makes sure this.isValueChanged is set to True the 2nd time the value is changed
                if (!this.isInitialValue && !this.isValueChanged)
                    this.isValueChanged = true;

                this.isInitialValue = false;

                if (value != null && value.Split(';').Count() > 0)
                {
                    this.values = value.Split(';');
                }

                SetProperty<string>(ref this.value, value);
                OnPropertyChanged(() => this.Values);
                OnPropertyChanged(() => this.IsValueChanged);
                OnPropertyChanged(() => this.IsNumeric);
            }
        }

        public string[] Values
        {
            get { return this.values; }

            set
            {
                // This makes sure this.isValueChanged is set to True the 2nd time the value is changed
                if (!this.isInitialValue)
                    this.isValueChanged = true;

                this.isInitialValue = false;

                if (value != null && value.Count() > 0)
                {
                    this.value = string.Join(";", value);
                }

                SetProperty<string[]>(ref this.values, value);
                OnPropertyChanged(() => this.Value);
                OnPropertyChanged(() => this.IsValueChanged);
                OnPropertyChanged(() => this.IsNumeric);
            }
        }
        #endregion
    }
}
