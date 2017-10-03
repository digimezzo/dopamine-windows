using SQLite;

namespace Dopamine.Common.Database.Entities
{
    public class Folder
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long FolderID { get; set; }
        public string Path { get; set; }
        public string SafePath { get; set; }
        public long ShowInCollection { get; set; }
        #endregion
    }
}
