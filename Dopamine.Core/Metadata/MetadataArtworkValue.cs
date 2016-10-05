
using Prism.Mvvm;

namespace Dopamine.Core.Metadata
{
    public class MetadataArtworkValue : BindableBase
    {
        #region Private
        private string pathValue;
        private byte[] dataValue;
        private bool isInitialValue = true;
        private bool isValueChanged;
        #endregion

        #region Readonly Properties
        public bool IsValueChanged
        {
            get { return this.isValueChanged; }
        }
        #endregion

        #region Properties
        public string PathValue
        {
            get { return this.pathValue; }
        }

        public byte[] DataValue
        {
            get { return this.dataValue; }
        }
        #endregion

        #region Public
        public void SetValue(string pathValue, byte[] dataValue)
        {
            // This makes sure this.isValueChanged is set to True the 2nd time the value is changed
            if (!this.isInitialValue)
                this.isValueChanged = true;

            this.isInitialValue = false;

            this.pathValue = pathValue;
            this.dataValue = dataValue;

            OnPropertyChanged(() => this.PathValue);
            OnPropertyChanged(() => this.DataValue);
            OnPropertyChanged(() => this.IsValueChanged);
        }
        #endregion
    }
}
