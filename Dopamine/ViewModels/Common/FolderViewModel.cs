using Dopamine.Common.Database.Entities;
using Prism.Mvvm;

namespace Dopamine.ViewModels.Common
{
    public class FolderViewModel : BindableBase
    {
        private Folder folder;

        public Folder Folder
        {
            get { return this.folder; }
            set
            {
                SetProperty<Folder>(ref this.folder, value);
                RaisePropertyChanged(nameof(this.Path));
                RaisePropertyChanged(nameof(this.Directory));
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
                this.Folder.ShowInCollection = value ? 1 : 0;
                RaisePropertyChanged(nameof(this.ShowInCollection));
            }
        }
    }
}
