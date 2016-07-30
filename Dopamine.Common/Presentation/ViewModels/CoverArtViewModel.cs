using Prism.Mvvm;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class CoverArtViewModel : BindableBase
    {
        #region Variables
        private Image coverArt;
        #endregion

        #region Properties
        public Image CoverArt
        {
            get { return this.coverArt; }
            set { SetProperty<Image>(ref this.coverArt, value); }
        }
        #endregion
    }
}
