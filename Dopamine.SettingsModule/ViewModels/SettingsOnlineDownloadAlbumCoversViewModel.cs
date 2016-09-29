using Dopamine.Core.Settings;
using Prism.Mvvm;
using System.Threading.Tasks;
namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsOnlineDownloadAlbumCoversViewModel : BindableBase
    {
        #region Variables
        private bool checkBoxDownloadMissingAlbumCoversChecked;
        private bool checkBoxEmbedDownloadedCoversInAudioFilesChecked;
        #endregion

        #region Properties
        public bool CheckBoxDownloadMissingAlbumCoversChecked
        {
            get { return this.checkBoxDownloadMissingAlbumCoversChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Lastfm", "DownloadAlbumCovers", value);
                SetProperty<bool>(ref this.checkBoxDownloadMissingAlbumCoversChecked, value);
            }
        }

        public bool CheckBoxEmbedDownloadedCoversInAudioFilesChecked
        {
            get { return this.checkBoxEmbedDownloadedCoversInAudioFilesChecked; }
            set
            {
                XmlSettingsClient.Instance.Set<bool>("Lastfm", "EmbedAlbumCoversInAudioFiles", value);
                SetProperty<bool>(ref this.checkBoxEmbedDownloadedCoversInAudioFilesChecked, value);
            }
        }
        #endregion

        #region Construction
        public SettingsOnlineDownloadAlbumCoversViewModel()
        {
            this.GetCheckBoxesAsync();
        }
        #endregion

        #region Private
        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.CheckBoxDownloadMissingAlbumCoversChecked = XmlSettingsClient.Instance.Get<bool>("Lastfm", "DownloadAlbumCovers");
                this.CheckBoxEmbedDownloadedCoversInAudioFilesChecked = XmlSettingsClient.Instance.Get<bool>("Lastfm", "EmbedAlbumCoversInAudioFiles");

            });
        }
        #endregion
    }
}
