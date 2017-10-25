using Prism.Mvvm;

namespace Dopamine.Common.Metadata
{
    public class MetadataRatingValue : BindableBase
    {
        private int value;
        private bool isValueChanged;


        public bool IsValueChanged
        {
            get { return this.isValueChanged; }
        }
  
        public int Value
        {
            get { return this.value; }

            set
            {
                this.value = value;
                this.isValueChanged = true;
                this.OnPropertiesChanged();
            }
        }
        public MetadataRatingValue()
        {
        }

        public MetadataRatingValue(int value)
        {
            this.value = value;
            this.OnPropertiesChanged();
        }
        private void OnPropertiesChanged()
        {
            RaisePropertyChanged(nameof(this.Value));
            RaisePropertyChanged(nameof(this.IsValueChanged));
        }
    }
}
