using Dopamine.Common.Database.Entities;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels.Entities
{
    public class PlaylistViewModel : BindableBase
    {
        #region Variables
        private Playlist playlist;
        #endregion

        #region Properties
        public Playlist Playlist
        {
            get { return this.playlist; }
            set { SetProperty<Playlist>(ref this.playlist, value); }
        }

        public string PlaylistName
        {
            get { return this.Playlist.PlaylistName; }
            set {
                this.Playlist.PlaylistName = value;
                OnPropertyChanged(() => this.PlaylistName);
            }
        }
        #endregion

        #region Public
        public override string ToString()
        {
            return this.PlaylistName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Playlist.Equals(((PlaylistViewModel)obj).Playlist);
        }

        public override int GetHashCode()
        {
            return this.playlist.GetHashCode();
        }
        #endregion
    }

}
