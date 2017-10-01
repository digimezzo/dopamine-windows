namespace Dopamine.Common.Services.Indexing
{
    public delegate void FolderChangedEventHandler(string folder);

    public class FolderWatcher
    {
        #region Variables

        #endregion

        #region Events
        event FolderChangedEventHandler FolderChanged = delegate { };
        #endregion

        #region Public
        public void StartWatching()
        {

        }

        public void StopWatching()
        {

        }

        public void RestartWatching()
        {
            this.StopWatching();
            this.StartWatching();
        }
        #endregion
    }
}
