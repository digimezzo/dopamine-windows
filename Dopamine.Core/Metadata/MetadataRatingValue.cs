using Microsoft.Practices.Prism.Mvvm;
namespace Dopamine.Core.Metadata
{
    public class MetadataRatingValue : BindableBase
    {
        #region Private
        private int value;
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
        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                // This makes sure this.isValueChanged is set to True the 2nd time the value is changed
                if (!this.isInitialValue && !this.isValueChanged)
                    this.isValueChanged = true;

                this.isInitialValue = false;

                SetProperty<int>(ref this.value, value);
                OnPropertyChanged(() => this.IsValueChanged);
            }
        }
        #endregion
    }
}
