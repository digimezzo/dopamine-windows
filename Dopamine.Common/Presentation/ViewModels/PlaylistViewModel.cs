using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaylistViewModel : BindableBase
    {
        #region Variables
        private string playlist;
        #endregion

        #region Properties
        public string Playlist
        {
            get { return playlist; }
            set { playlist = value; }
        }

        public string SortPlaylist
        {
           get { return playlist.ToLowerInvariant(); }
        }
        #endregion

        #region Public
        public override string ToString()
        {
            return this.Playlist;
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
            return this.Playlist.GetHashCode();
        }
        #endregion
    }
}
