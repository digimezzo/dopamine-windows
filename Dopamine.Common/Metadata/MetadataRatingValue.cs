using Prism.Mvvm;

namespace Dopamine.Common.Metadata
{
    public class MetadataRatingValue : BindableBase
    {
        #region Private
        private int value;
        private bool isValueChanged;
        #endregion

        #region Readonly Properties
        public bool IsValueChanged
        {
            get { return this.isValueChanged; }
        }
        #endregion

        #region Properties
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
        #endregion

        #region Construction
        public MetadataRatingValue()
        {
        }

        public MetadataRatingValue(int value)
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
