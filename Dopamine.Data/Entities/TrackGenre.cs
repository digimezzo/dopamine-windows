using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("TrackGenres")]
    public class TrackGenre
    {
        [Column("track_id"), Indexed(), NotNull()]
        public long TrackId { get; set; }

        [Column("genre_id"), Indexed(), NotNull()]
        public long GenreId { get; set; }

    }
}
