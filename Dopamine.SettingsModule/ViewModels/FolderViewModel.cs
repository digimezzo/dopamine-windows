using Dopamine.Common.Database.Entities;
using Prism.Mvvm;

namespace Dopamine.SettingsModule.ViewModels
{
    public class FolderViewModel : BindableBase
    {
        #region Variables
        private Folder folder;
        #endregion

        #region Properties
        public Folder Folder
        {
            get { return this.folder; }
            set
            {
                SetProperty<Folder>(ref this.folder, value);
                OnPropertyChanged(() => this.Path);
                OnPropertyChanged(() => this.Directory);
            }
        }

        public string Path
        {
            get { return this.Folder.Path; }
        }

        public string Directory
        {
            get { return System.IO.Path.GetFileName(this.Folder.Path); }
        }

        public bool ShowInCollection
        {
            get { return this.Folder.ShowInCollection == 1 ? true : false; }

            set
            {
                if (value)
                {
                    this.Folder.ShowInCollection = 1;
                }
                else
                {
                    this.Folder.ShowInCollection = 0;
                }

                OnPropertyChanged(() => this.ShowInCollection);
            }
        }
        #endregion
    }
}
