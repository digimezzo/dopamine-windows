using Prism.Mvvm;

namespace Dopamine.ViewModels
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