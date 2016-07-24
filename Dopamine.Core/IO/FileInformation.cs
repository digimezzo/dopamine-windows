using System;

namespace Dopamine.Core.IO
{
    public class FileInformation
    {
        #region Variables
        private string filePath;
        #endregion

        #region Construction
        public FileInformation(string filePath)
        {
            this.filePath = filePath;
        }
        #endregion

        #region Readonly Properties
        public string Folder
        {
            get { return System.IO.Path.GetDirectoryName(this.filePath); }
        }

        public string Name
        {
            get { return System.IO.Path.GetFileName(this.filePath); }
        }

        public string NameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(this.filePath); }
        }

        public long SizeInBytes
        {
            get { return FileOperations.GetFileSize(this.filePath); }
        }

        public DateTime DateModified
        {
            get { return new DateTime(FileOperations.GetDateModified(this.filePath)); }
        }

        public long DateModifiedTicks
        {
            get { return FileOperations.GetDateModified(this.filePath); }
        }
        #endregion
    }
}
