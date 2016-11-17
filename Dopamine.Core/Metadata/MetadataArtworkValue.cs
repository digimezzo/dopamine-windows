using Prism.Mvvm;

namespace Dopamine.Core.Metadata
{
    public class MetadataArtworkValue : BindableBase
    {
        #region Private
        private byte[] value;
        private bool isValueChanged;
        private string artworkID;
        #endregion

        #region Readonly Properties
        public bool IsValueChanged
        {
            get { return this.isValueChanged; }
        }
        #endregion

        #region Properties
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

        public string ArtworkID
        {
            get
            {
                if (this.artworkID == null) this.artworkID = string.Empty;
                return this.artworkID;
            }
            set
            {
                SetProperty<string>(ref this.artworkID, value);
            }
        }
        #endregion

        #region Construction
        public MetadataArtworkValue()
        {
        }

        public MetadataArtworkValue(byte[] value)
        {
            this.value = value;
            this.OnPropertiesChanged();
        }
        #endregion

        #region Private
        private void OnPropertiesChanged()
        {
            OnPropertyChanged(() => this.Value);
            OnPropertyChanged(() => this.IsValueChanged);
        }
        #endregion
    }
}
