using SQLite.Net.Attributes;

namespace Dopamine.Core.Database.Entities
{
    public class Folder
    {
        #region Properties
        [PrimaryKey()]
        public long FolderID { get; set; }
        public string Path { get; set; }
        public long ShowInCollection { get; set; }
        #endregion
    }
}
