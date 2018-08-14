using Dopamine.Data.Entities;
using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class FolderViewModel : BindableBase
    {
        private Folder folder;

        public FolderViewModel(Folder folder)
        {
            this.folder = folder;
        }

        public Folder Folder => this.folder;

        public string Path => this.folder.Path;

        public string SafePath => this.folder.SafePath;

        public long FolderId => this.folder.FolderID;
       
        public string Directory => System.IO.Path.GetFileName(this.folder.Path);

        public bool ShowInCollection
        {
            get { return this.folder.ShowInCollection == 1 ? true : false; }

            set
            {
                this.folder.ShowInCollection = value ? 1 : 0;
                RaisePropertyChanged(nameof(this.ShowInCollection));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return string.Equals(this.SafePath, ((FolderViewModel)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return this.SafePath.GetHashCode();
        }

        public override string ToString()
        {
            return this.Directory; 
        }
    }
}
