using Prism.Mvvm;

namespace Dopamine.Data.Metadata
{
    public class MetadataArtworkValue : BindableBase
    {
        private byte[] value;
        private bool isValueChanged;
  
        public bool IsValueChanged
        {
            get { return this.isValueChanged; }
        }
   
        public byte[] Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.isValueChanged = true;
                this.OnPropertiesChanged();
            }
        }
 
        public MetadataArtworkValue()
        {
        }

        public MetadataArtworkValue(byte[] value)
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
