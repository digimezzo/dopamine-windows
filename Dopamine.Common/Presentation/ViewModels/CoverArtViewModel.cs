using Prism.Mvvm;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.ViewModels
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