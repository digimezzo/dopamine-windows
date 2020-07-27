using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("Artists")]
    public class Artist
    {
        public Artist() { }

        [Column("id"), PrimaryKey(), AutoIncrement()]
        public long Id { get; set; }

        [Column("name"), Unique(), Collation("NOCASE"), NotNull()]
        public string Name { get; set; }

    }
}
