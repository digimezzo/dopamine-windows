using Prism.Mvvm;
using System.Linq;

namespace Dopamine.Services.Contracts.Metadata
{
    public class MetadataValue : BindableBase
    {
        private string value;
        private string[] values;
        private bool isValueChanged;
       
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
    
        public string Value
        {
            get { return this.value; }

            set
            {
                this.value = value;
                this.values = this.ConvertToValues(value);
                this.isValueChanged = true;
                this.OnPropertiesChanged();
            }
        }

        public string[] Values
        {
            get { return this.values; }

            set
            {
                this.values = value;
                this.value = ConvertToValue(value);
                this.isValueChanged = true;
                this.OnPropertiesChanged();
            }
        }
       
        public MetadataValue()
        {
        }

        public MetadataValue(string value)
        {
            this.value = value != null ? value : string.Empty;
            this.values = this.ConvertToValues(value);
            this.OnPropertiesChanged();
        }

        public MetadataValue(uint value)
        {
            this.value = value == 0 ? string.Empty : value.ToString();
            this.values = null;
            this.OnPropertiesChanged();
        }

        public MetadataValue(string[] values)
        {
            this.values = values;
            this.value = ConvertToValue(values);
            this.OnPropertiesChanged();
        }
   
        private void OnPropertiesChanged()
        {
            RaisePropertyChanged(nameof(this.Value));
            RaisePropertyChanged(nameof(this.Values));
            RaisePropertyChanged(nameof(this.IsValueChanged));
            RaisePropertyChanged(nameof(this.IsNumeric));
        }

        private string[] ConvertToValues(string value)
        {
            if (value != null && value.Split(';').Count() > 0) return this.values = value.Split(';');
            return null;
        }

        private string ConvertToValue(string[] values)
        {
            if (values != null && values.Count() > 0) return string.Join(";", values);
            return string.Empty;
        }
    }
}
