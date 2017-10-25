using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels.Entities
{
    public class CoverArtViewModel : BindableBase
    {
        private byte[] coverArt;

        public byte[] CoverArt
        {
            get { return this.coverArt; }
            set { SetProperty<byte[]>(ref this.coverArt, value); }
        }
    }
}