using Dopamine.Presentation.Interfaces;
using Prism.Mvvm;

namespace Dopamine.Presentation.ViewModels
{
    public class SemanticZoomSelectorViewModel : BindableBase, ISemanticZoomSelector
    {
        private string header;
        private bool canZoom;

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

        public override int GetHashCode()
        {
            return this.Header.GetHashCode();
        }
    }
}