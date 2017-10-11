using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels.Entities
{
    public class CoverArtViewModel : BindableBase
    {
        #region Variables
        private byte[] coverArt;
        #endregion

        #region Properties
        public byte[] CoverArt
        {
            get { return this.coverArt; }
            set { SetProperty<byte[]>(ref this.coverArt, value); }
        }
        #endregion
    }
}