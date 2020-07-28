using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("Albums")]
    public class Album
    {
        [Column("id"), PrimaryKey(), AutoIncrement()]
        public long Id { get; set; }

        [Column("name"), Unique(), Collation("NOCASE"), NotNull()]
        public string Name { get; set; }

    }
}
