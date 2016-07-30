using Dopamine.Common.Presentation.Interfaces;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class SemanticZoomSelectorViewModel : BindableBase, ISemanticZoomSelector
    {
        #region Variables
        private string header;
        private bool canZoom;
        #endregion

        #region Properties
        public string Header
        {
            get { return this.header; }
            set { SetProperty<string>(ref this.header, value); }
        }


        public bool CanZoom
        {
            get { return this.canZoom; }
            set { SetProperty<bool>(ref this.canZoom, value); }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {

            return this.Header;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Header.ToLower().Equals(((SemanticZoomSelectorViewModel)obj).Header.ToLower());
        }
        #endregion
    }
}
