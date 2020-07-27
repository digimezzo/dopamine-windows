using SQLite;

namespace Dopamine.Data.Entities
{
    [Table("Folders")]
    public class Folder2
    {
        [Column("id"), PrimaryKey(), AutoIncrement()]
        public long Id { get; set; }

        [Column("path"), Unique(), Collation("NOCASE"), NotNull()]
        public string Path { get; set; }

        [Column("show"), Indexed(), NotNull()]
        public long Show { get; set; }
    }
}
